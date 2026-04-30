import { useEffect, useMemo, useRef, useState } from "react";
import { MapContainer, TileLayer, Marker, useMap, useMapEvents } from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import { api } from "../api/client";
import { createSighting, getSpeciesByName } from "../api/library";

const pickerPinIcon = L.divIcon({
  className: "save-picker-pin",
  html: `
    <div class="save-picker-pin-shadow"></div>
    <div class="save-picker-pin-body">
      <svg viewBox="0 0 24 24" fill="white" stroke="white" stroke-width="0">
        <path d="M12 2C7.6 2 4 5.6 4 10c0 5.5 7 12 8 12s8-6.5 8-12c0-4.4-3.6-8-8-8zm0 11a3 3 0 1 1 0-6 3 3 0 0 1 0 6z"/>
      </svg>
    </div>
  `,
  iconSize: [36, 44],
  iconAnchor: [18, 42],
});

// Pans the picker map only when coords change from outside the map itself
// (typed input or "Use my location"). Direct map clicks set skipRef so we
// don't yank the view from under the user's tap.
function MapPanHandler({ center, skipRef }) {
  const map = useMap();
  useEffect(() => {
    if (!center) return;
    if (skipRef.current) {
      skipRef.current = false;
      return;
    }
    map.flyTo(center, Math.max(map.getZoom(), 12), { duration: 0.6 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [map, center?.[0], center?.[1]]);
  return null;
}

function MapClickHandler({ onPick }) {
  useMapEvents({
    click: (e) => onPick(e.latlng.lat, e.latlng.lng),
  });
  return null;
}

const SparkleIcon = (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M12 2L15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2z" />
  </svg>
);

const PinIcon = (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
    <circle cx="12" cy="10" r="3" />
  </svg>
);

const MapIcon = (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <polygon points="1 6 1 22 8 18 16 22 23 18 23 2 16 6 8 2 1 6" />
    <line x1="8" y1="2" x2="8" y2="18" />
    <line x1="16" y1="6" x2="16" y2="22" />
  </svg>
);

const SaveIcon = (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z" />
    <polyline points="17 21 17 13 7 13 7 21" />
    <polyline points="7 3 7 8 15 8" />
  </svg>
);

// Try hard to extract a JSON object from a free-form AI response.
// Handles ```json ... ``` fences and prose around the JSON.
function extractJsonObject(text) {
  if (!text || typeof text !== "string") return null;
  const cleaned = text.replace(/```json|```/gi, "").trim();
  const start = cleaned.indexOf("{");
  const end = cleaned.lastIndexOf("}");
  if (start === -1 || end === -1 || end <= start) return null;
  try {
    return JSON.parse(cleaned.slice(start, end + 1));
  } catch {
    return null;
  }
}

/** Groq sometimes puts "true"/"false" strings instead of JSON booleans. */
function coerceBool(v) {
  if (typeof v === "boolean") return v;
  if (v === "true" || v === "false") return v === "true";
  return null;
}

// Resize and JPEG-compress an image File client-side, then return a base64
// data: URL. Keeps the saved image small enough not to bloat the database
// while still looking good on the library card.
async function compressImageToDataUrl(file, maxDim = 800, quality = 0.78) {
  if (!file) return null;
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onerror = () => reject(reader.error);
    reader.onload = () => {
      const img = new Image();
      img.onerror = () => reject(new Error("Could not decode image"));
      img.onload = () => {
        const scale = Math.min(1, maxDim / Math.max(img.width, img.height));
        const w = Math.max(1, Math.round(img.width * scale));
        const h = Math.max(1, Math.round(img.height * scale));
        const canvas = document.createElement("canvas");
        canvas.width = w;
        canvas.height = h;
        const ctx = canvas.getContext("2d");
        ctx.drawImage(img, 0, 0, w, h);
        resolve(canvas.toDataURL("image/jpeg", quality));
      };
      img.src = reader.result;
    };
    reader.readAsDataURL(file);
  });
}

function nowLocalDateTimeInput() {
  const d = new Date();
  d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
  return d.toISOString().slice(0, 16);
}

export default function SaveSightingModal({
  isOpen,
  onClose,
  initialAnimalName,
  sessionId,
  recognizedImage,
  onSaved,
}) {
  const [animalName, setAnimalName] = useState(initialAnimalName || "");
  const [imageUrl, setImageUrl] = useState("");
  const [latitude, setLatitude] = useState("");
  const [longitude, setLongitude] = useState("");
  const [notes, setNotes] = useState("");
  const [sightedAt, setSightedAt] = useState(nowLocalDateTimeInput());
  const [saving, setSaving] = useState(false);
  const [autoFilling, setAutoFilling] = useState(false);
  const [locating, setLocating] = useState(false);
  const [pickerOpen, setPickerOpen] = useState(false);
  const [error, setError] = useState("");
  // Soft hint shown above the Save button when the typed name doesn't
  // resolve to a catalogued species. We still allow the save in that case.
  const [unknownSpeciesNotice, setUnknownSpeciesNotice] = useState("");
  const skipPanRef = useRef(false);

  // Center the picker on the existing typed coords (or world view).
  const pickerCenter = useMemo(() => {
    const lat = parseFloat(latitude);
    const lng = parseFloat(longitude);
    if (Number.isFinite(lat) && Number.isFinite(lng) && Math.abs(lat) <= 90 && Math.abs(lng) <= 180) {
      return [lat, lng];
    }
    return null;
  }, [latitude, longitude]);

  const handlePick = (lat, lng) => {
    skipPanRef.current = true; // user clicked / dragged the marker — keep view as-is
    setLatitude(lat.toFixed(6));
    setLongitude(lng.toFixed(6));
    setError("");
  };

  useEffect(() => {
    if (!isOpen) return;
    setAnimalName(initialAnimalName || "");
    setImageUrl("");
    setLatitude("");
    setLongitude("");
    setNotes("");
    setSightedAt(nowLocalDateTimeInput());
    setError("");
    setUnknownSpeciesNotice("");
    setSaving(false);
    setAutoFilling(false);
    setLocating(false);
    setPickerOpen(false);
  }, [isOpen, initialAnimalName]);

  // Whenever the user edits the species name, drop the stale "not in our
  // catalogue" hint so the UI doesn't lie about the freshly-typed value.
  useEffect(() => {
    setUnknownSpeciesNotice("");
  }, [animalName]);

  if (!isOpen) return null;

  const useMyLocation = () => {
    if (!navigator.geolocation) {
      setError("Geolocation is not supported by your browser.");
      return;
    }
    setLocating(true);
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        setLatitude(pos.coords.latitude.toFixed(6));
        setLongitude(pos.coords.longitude.toFixed(6));
        setLocating(false);
        setError("");
      },
      (err) => {
        setLocating(false);
        setError("Could not access your location: " + err.message);
      },
      { enableHighAccuracy: true, timeout: 10000 }
    );
  };

  const aiAutoFill = async () => {
    if (!animalName.trim()) {
      setError("Enter the animal name first so the AI knows what to describe.");
      return;
    }
    if (!sessionId) {
      setError("AI session is not active. Re-analyze the photo and try again.");
      return;
    }
    setAutoFilling(true);
    setError("");
    try {
      // 1) Image: reuse the photo the user already uploaded for recognition.
      //    No AI involvement — we just compress it and stash it as a data URL.
      if (recognizedImage) {
        try {
          const dataUrl = await compressImageToDataUrl(recognizedImage);
          if (dataUrl) setImageUrl(dataUrl);
        } catch {
          // non-fatal — leave the image field as-is
        }
      }

      // 2) Notes: strict metadata only — color + dangerous + rare (no prose).
      const species = animalName.trim();
      const prompt =
        `Library metadata ONLY. Reply with EXACTLY one JSON object — no markdown fences, no code blocks, no text before or after the JSON.\n` +
        `Allowed keys ONLY: "color" (string), "dangerous" (boolean), "rare" (boolean).\n` +
        `Shape: {"color":"<2-10 words: typical coat/feather/scale colours>","dangerous":true|false,"rare":true|false}\n` +
        `Species: "${species}".\n` +
        `Rules: "dangerous" means risk to PEOPLE (physical injury/death in realistic encounters), NOT habitat loss or hunting pressure — true for large predators, hippos, crocs, venomous snakes, etc.; otherwise false. ` +
        `"rare" = true if threatened/endangered or small/fragmented wild population; otherwise false.\n` +
        `Do NOT add habitat stories, behaviour paragraphs, or any key besides color, dangerous, rare.`;

      const res = await api.post(`/api/ai/ask/${sessionId}`, {
        questionAboutNature: prompt,
      });
      const answer = res.data?.answer ?? "";
      const parsed = extractJsonObject(answer);
      const dangerous = coerceBool(parsed?.dangerous);
      const rare = coerceBool(parsed?.rare);
      if (parsed && typeof parsed.color === "string" && dangerous !== null && rare !== null) {
        const line = `Color: ${parsed.color.trim()} | Risk to humans: ${dangerous ? "Yes" : "No"} | Rare (wild pop.): ${rare ? "Yes" : "No"}`;
        setNotes(line.slice(0, 500));
      } else {
        setError(
          'AI auto-fill must return JSON like {"color":"…","dangerous":false,"rare":true} only. Tap AI auto-fill again.'
        );
      }
    } catch (err) {
      setError("AI auto-fill failed: " + (err.response?.data?.message || err.message));
    } finally {
      setAutoFilling(false);
    }
  };

  const submit = async () => {
    setError("");
    setUnknownSpeciesNotice("");
    const lat = parseFloat(latitude);
    const lng = parseFloat(longitude);
    const trimmedName = animalName.trim();
    if (!trimmedName) return setError("Animal name is required.");
    if (!Number.isFinite(lat) || lat < -90 || lat > 90)
      return setError("Latitude must be a number between -90 and 90.");
    if (!Number.isFinite(lng) || lng < -180 || lng > 180)
      return setError("Longitude must be a number between -180 and 180.");
    if (notes.length > 500) return setError("Notes can be at most 500 characters.");

    setSaving(true);
    try {
      // Try to link to a catalogued species — if found we get the rich
      // metadata (rarity, danger, scientific name) for free. If not, we
      // still save it as a free-form / custom entry: the backend now
      // accepts a payload without a SpeciesId.
      let speciesId = null;
      let scientificName = null;
      try {
        const species = await getSpeciesByName(trimmedName);
        if (species) {
          speciesId = species.id;
          scientificName = species.scientificName || null;
        } else {
          setUnknownSpeciesNotice(
            `"${trimmedName}" isn't in our curated catalogue — we'll save it as a custom entry.`
          );
        }
      } catch {
        // Lookup failure is non-fatal: fall through to a free-form save.
      }

      const created = await createSighting({
        speciesId,
        commonName: trimmedName,
        scientificName,
        latitude: lat,
        longitude: lng,
        imageUrl: imageUrl.trim() || null,
        notes: notes.trim() || null,
        sightedAt: sightedAt ? new Date(sightedAt).toISOString() : null,
      });
      onSaved?.(created);
      onClose?.();
    } catch (err) {
      setError("Save failed: " + (err.response?.data?.message || err.message));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="ai-modal-overlay" onClick={onClose}>
      <div className="save-modal" onClick={(e) => e.stopPropagation()}>
        <button className="ai-modal-close" onClick={onClose} aria-label="Close">×</button>

        <div className="ai-modal-header">
          <div className="ai-modal-icon">{SaveIcon}</div>
          <h3>Save to Library</h3>
          <p>Add this encounter to your personal wildlife collection</p>
        </div>

        <div className="ai-modal-body save-modal-body">
          <div className="save-field">
            <label className="ai-label">Species</label>
            <input
              className="save-input"
              type="text"
              value={animalName}
              onChange={(e) => setAnimalName(e.target.value)}
              placeholder="e.g. Lion"
              maxLength={100}
            />
          </div>

          <div className="save-field-row">
            <div className="save-field">
              <label className="ai-label">Latitude</label>
              <input
                className="save-input"
                type="number"
                step="0.000001"
                min={-90}
                max={90}
                value={latitude}
                onChange={(e) => setLatitude(e.target.value)}
                placeholder="-90 to 90"
              />
            </div>
            <div className="save-field">
              <label className="ai-label">Longitude</label>
              <input
                className="save-input"
                type="number"
                step="0.000001"
                min={-180}
                max={180}
                value={longitude}
                onChange={(e) => setLongitude(e.target.value)}
                placeholder="-180 to 180"
              />
            </div>
          </div>

          <div className="save-locator-actions">
            <button
              type="button"
              className="save-secondary-btn"
              onClick={useMyLocation}
              disabled={locating}
            >
              {locating ? <span className="ai-spinner small"></span> : PinIcon}
              {locating ? "Locating…" : "Use my location"}
            </button>
            <button
              type="button"
              className={`save-secondary-btn ${pickerOpen ? "active" : ""}`}
              onClick={() => setPickerOpen((v) => !v)}
            >
              {MapIcon}
              {pickerOpen ? "Hide map" : "Pick on map"}
            </button>
          </div>

          {pickerOpen && (
            <div className="save-map-picker">
              <MapContainer
                center={pickerCenter || [20, 0]}
                zoom={pickerCenter ? 12 : 2}
                style={{ height: 280, width: "100%" }}
                scrollWheelZoom={true}
              >
                <TileLayer
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                />
                <MapClickHandler onPick={handlePick} />
                <MapPanHandler center={pickerCenter} skipRef={skipPanRef} />
                {pickerCenter && (
                  <Marker
                    position={pickerCenter}
                    icon={pickerPinIcon}
                    draggable={true}
                    eventHandlers={{
                      dragend: (e) => {
                        const { lat, lng } = e.target.getLatLng();
                        handlePick(lat, lng);
                      },
                    }}
                  />
                )}
              </MapContainer>
              <p className="save-map-hint">
                Click the map to drop a pin · drag the pin to fine-tune
              </p>
            </div>
          )}

          <div className="save-field">
            <label className="ai-label">
              Image{" "}
              <span className="save-hint">
                (auto-filled with your uploaded photo when you tap AI auto-fill)
              </span>
            </label>
            {imageUrl ? (
              <div className="save-image-preview">
                <img src={imageUrl} alt="Selected" />
                <button
                  type="button"
                  className="save-image-clear"
                  onClick={() => setImageUrl("")}
                  aria-label="Remove image"
                >
                  ×
                </button>
              </div>
            ) : (
              <input
                className="save-input"
                type="url"
                value={imageUrl}
                onChange={(e) => setImageUrl(e.target.value)}
                placeholder="Paste an image URL, or use AI auto-fill below"
              />
            )}
          </div>

          <div className="save-field">
            <label className="ai-label">
              Notes <span className="save-hint">({notes.length}/500)</span>
            </label>
            <textarea
              className="ai-textarea"
              rows={3}
              value={notes}
              onChange={(e) => setNotes(e.target.value.slice(0, 500))}
              placeholder="Where did you see it? What was it doing?"
            />
          </div>

          <div className="save-field">
            <label className="ai-label">Sighted at</label>
            <input
              className="save-input"
              type="datetime-local"
              value={sightedAt}
              max={nowLocalDateTimeInput()}
              onChange={(e) => setSightedAt(e.target.value)}
            />
          </div>

          <button
            type="button"
            className="save-ai-btn"
            onClick={aiAutoFill}
            disabled={autoFilling || !animalName.trim()}
          >
            {autoFilling ? (
              <>
                <span className="ai-spinner small"></span>
                AI is thinking…
              </>
            ) : (
              <>
                {SparkleIcon}
                AI auto-fill
              </>
            )}
          </button>

          {unknownSpeciesNotice && (
            <div className="save-info-notice">{unknownSpeciesNotice}</div>
          )}
          {error && <div className="save-error">{error}</div>}
        </div>

        <div className="save-modal-actions">
          <button className="btn btn-outline" onClick={onClose} disabled={saving}>
            Cancel
          </button>
          <button className="btn btn-primary" onClick={submit} disabled={saving || autoFilling}>
            {saving ? (
              <>
                <span className="ai-spinner small"></span>
                Saving…
              </>
            ) : (
              "Save to library"
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
