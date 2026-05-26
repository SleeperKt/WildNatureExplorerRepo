import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  MapContainer,
  TileLayer,
  Marker,
  Popup,
  Circle,
  useMap,
} from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import Header from '../components/Header';
import Footer from '../components/Footer';
import {
  deleteSighting,
  getMyLibrary,
  getNearbySightings,
} from '../api/library';

// Reuse the GeoPage marker style so the Library map looks identical to /geo.
const createAnimalMarker = (name, category) => {
  const colors = {
    dangerous: { bg: '#ef4444', border: '#dc2626' },
    rare: { bg: '#8b5cf6', border: '#7c3aed' },
    common: { bg: '#22c55e', border: '#16a34a' },
    // Free-form / not-in-catalogue entries: amber so they match the "?"
    // badge on the cards and don't masquerade as a known category.
    unknown: { bg: '#f59e0b', border: '#d97706' },
  };
  const color = colors[category] || colors.common;
  const displayName = name.length > 12 ? name.substring(0, 10) + '...' : name;

  return L.divIcon({
    className: 'animal-marker-container',
    html: `
      <div class="animal-marker ${category}">
        <div class="animal-marker-circle" style="background: ${color.bg}; border-color: ${color.border}">
          <svg viewBox="0 0 24 24" fill="white" width="20" height="20">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
          </svg>
        </div>
        <div class="animal-marker-name" style="background: ${color.bg}">${displayName}</div>
      </div>
    `,
    iconSize: [80, 60],
    iconAnchor: [40, 50],
    popupAnchor: [0, -50],
  });
};

const userLocationIcon = L.divIcon({
  className: 'user-location-marker',
  html: `<div class="user-marker-dot"></div><div class="user-marker-pulse"></div>`,
  iconSize: [24, 24],
  iconAnchor: [12, 12],
});

function MapAutoFit({ markers, center, fallbackZoom = 4 }) {
  const map = useMap();

  useEffect(() => {
    if (markers && markers.length > 0) {
      const bounds = L.latLngBounds(
        markers.map((m) => [m.latitude, m.longitude])
      );
      if (center) bounds.extend(center);
      map.fitBounds(bounds, { padding: [60, 60], maxZoom: 12 });
    } else if (center) {
      map.setView(center, 8);
    } else {
      map.setView([20, 0], fallbackZoom);
    }
  }, [map, markers, center, fallbackZoom]);

  return null;
}

// Free-form entries (no catalogued Species link) don't carry IsDangerous /
// IsRare info, so we render an honest "Custom" badge instead of defaulting
// to "Common" — otherwise saving e.g. "Lion" would silently mislabel a
// dangerous animal as harmless.
const categoryOf = (s) => {
  if (!s.speciesId) return 'unknown';
  if (s.isDangerous) return 'dangerous';
  if (s.isRare) return 'rare';
  return 'common';
};
const categoryLabel = (s) => {
  if (!s.speciesId) return 'Custom';
  if (s.isDangerous) return 'Dangerous';
  if (s.isRare) return 'Rare';
  return 'Common';
};

function formatDate(iso) {
  if (!iso) return '—';
  try {
    return new Date(iso).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  } catch {
    return iso;
  }
}

function FlipCard({ sighting, onDelete }) {
  const [flipped, setFlipped] = useState(false);
  const [imgFailed, setImgFailed] = useState(false);
  const cat = categoryOf(sighting);

  // Free-form entries (no catalogued Species link) get a "?" badge in the
  // top-right corner so the user can spot them at a glance.
  const isUnknownSpecies = !sighting.speciesId;

  return (
    <div
      className={`lib-card ${flipped ? 'flipped' : ''}`}
      onClick={() => setFlipped((v) => !v)}
      role="button"
      tabIndex={0}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          setFlipped((v) => !v);
        }
      }}
    >
      <div className="lib-card-inner">
        {/* Front */}
        <div className="lib-card-face lib-card-front">
          {sighting.imageUrl && !imgFailed ? (
            <img
              src={sighting.imageUrl}
              alt={sighting.commonName}
              className="lib-card-image"
              onError={() => setImgFailed(true)}
            />
          ) : (
            <div className="lib-card-placeholder">
              <svg
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.5"
              >
                <rect x="3" y="3" width="18" height="18" rx="2" ry="2" />
                <circle cx="8.5" cy="8.5" r="1.5" />
                <polyline points="21 15 16 10 5 21" />
              </svg>
              <span>No image</span>
            </div>
          )}
          {isUnknownSpecies && (
            <span
              className="lib-card-unknown-badge"
              title="Not in our species catalogue — saved as a custom entry"
              aria-label="Custom entry — not in species catalogue"
            >
              ?
            </span>
          )}
          <div className="lib-card-front-overlay">
            <span className={`lib-card-badge cat-${cat}`}>
              {categoryLabel(sighting)}
            </span>
            <h3>{sighting.commonName}</h3>
            <p className="lib-card-scientific">{sighting.scientificName}</p>
          </div>
        </div>

        {/* Back: scrollable so long notes are fully readable. We stop
            click-propagation on the scrollable area so the user can pan/
            select text without accidentally flipping the card back. The
            small return button (top-left) and Remove button still work via
            their own onClick handlers. */}
        <div
          className="lib-card-face lib-card-back"
          onClick={(e) => e.stopPropagation()}
        >
          {isUnknownSpecies && (
            <span
              className="lib-card-unknown-badge on-back"
              title="Not in our species catalogue — saved as a custom entry"
              aria-label="Custom entry — not in species catalogue"
            >
              ?
            </span>
          )}

          <button
            type="button"
            className="lib-card-flip-back"
            onClick={() => setFlipped(false)}
            aria-label="Flip card back"
            title="Flip back"
          >
            <svg
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <polyline points="15 18 9 12 15 6" />
            </svg>
          </button>

          <div className="lib-card-back-scroll">
            <div className="lib-card-back-header">
              <span className={`lib-card-badge cat-${cat}`}>
                {categoryLabel(sighting)}
              </span>
              <h3>{sighting.commonName}</h3>
              <p className="lib-card-scientific">{sighting.scientificName}</p>
            </div>

            <dl className="lib-card-info">
              <div>
                <dt>Sighted</dt>
                <dd>{formatDate(sighting.sightedAt)}</dd>
              </div>
              <div>
                <dt>Location</dt>
                <dd>
                  {sighting.latitude.toFixed(4)},{' '}
                  {sighting.longitude.toFixed(4)}
                </dd>
              </div>
              {typeof sighting.distanceKm === 'number' && (
                <div>
                  <dt>Distance</dt>
                  <dd>{sighting.distanceKm.toFixed(2)} km</dd>
                </div>
              )}
              {sighting.notes && (
                <div className="lib-card-notes">
                  <dt>Notes</dt>
                  <dd>{sighting.notes}</dd>
                </div>
              )}
            </dl>

            <button
              className="lib-card-delete"
              onClick={(e) => {
                e.stopPropagation();
                onDelete(sighting);
              }}
            >
              <svg
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <polyline points="3 6 5 6 21 6" />
                <path d="M19 6l-2 14a2 2 0 0 1-2 2H9a2 2 0 0 1-2-2L5 6" />
                <path d="M10 11v6M14 11v6" />
                <path d="M9 6V4a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v2" />
              </svg>
              Remove
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

export default function LibraryPage() {
  const navigate = useNavigate();
  const [tab, setTab] = useState('collection'); // 'collection' | 'map'
  const [sightings, setSightings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Map mode: 'all' shows every saved sighting; 'radius' filters by km from
  // the user's current location (calls fn_user_nearby_sightings on the server).
  const [mapMode, setMapMode] = useState('all');
  const [userLocation, setUserLocation] = useState(null);
  const [radiusInput, setRadiusInput] = useState('25');
  const [radiusKm, setRadiusKm] = useState(25);
  const [nearby, setNearby] = useState(null);
  const [nearbyLoading, setNearbyLoading] = useState(false);
  const radiusDebounceRef = useRef(null);

  const isAuthed = useMemo(() => Boolean(localStorage.getItem('token')), []);

  useEffect(() => {
    if (!isAuthed) {
      navigate('/login');
      return;
    }
    let cancelled = false;
    (async () => {
      try {
        setLoading(true);
        const data = await getMyLibrary();
        if (!cancelled) setSightings(data);
      } catch (err) {
        if (!cancelled)
          setError(
            'Failed to load your library: ' +
              (err.response?.data?.message || err.message)
          );
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [isAuthed, navigate]);

  // Try to grab user location once, used for radius searches.
  useEffect(() => {
    if (!navigator.geolocation) return;
    navigator.geolocation.getCurrentPosition(
      (pos) =>
        setUserLocation({
          lat: pos.coords.latitude,
          lng: pos.coords.longitude,
        }),
      () => {},
      { enableHighAccuracy: true, timeout: 10000 }
    );
  }, []);

  // Debounced effect — only fires in "radius" mode and calls
  // fn_user_nearby_sightings server-side.
  useEffect(() => {
    if (mapMode !== 'radius') {
      setNearby(null);
      return;
    }
    clearTimeout(radiusDebounceRef.current);
    radiusDebounceRef.current = setTimeout(async () => {
      if (!radiusKm || radiusKm <= 0) {
        setNearby([]);
        return;
      }
      if (!userLocation) {
        setError('Allow location access to use the radius filter.');
        return;
      }
      try {
        setNearbyLoading(true);
        setError('');
        const data = await getNearbySightings(
          userLocation.lat,
          userLocation.lng,
          radiusKm
        );
        setNearby(data);
      } catch (err) {
        setError(
          'Radius search failed: ' +
            (err.response?.data?.message || err.message)
        );
      } finally {
        setNearbyLoading(false);
      }
    }, 350);
    return () => clearTimeout(radiusDebounceRef.current);
  }, [mapMode, radiusKm, userLocation]);

  const onDelete = async (sighting) => {
    if (!confirm(`Remove "${sighting.commonName}" from your library?`)) return;
    try {
      await deleteSighting(sighting.id);
      setSightings((prev) => prev.filter((s) => s.id !== sighting.id));
      setNearby((prev) =>
        prev ? prev.filter((s) => s.id !== sighting.id) : prev
      );
    } catch (err) {
      alert('Delete failed: ' + (err.response?.data?.message || err.message));
    }
  };

  const requestLocation = () => {
    if (!navigator.geolocation) {
      setError('Geolocation is not supported by your browser.');
      return;
    }
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        setUserLocation({
          lat: pos.coords.latitude,
          lng: pos.coords.longitude,
        });
        setError('');
      },
      (err) => setError('Could not access location: ' + err.message),
      { enableHighAccuracy: true, timeout: 10000 }
    );
  };

  const visibleOnMap = mapMode === 'radius' ? (nearby ?? []) : sightings;
  const radiusActive = mapMode === 'radius' && radiusKm > 0 && !!userLocation;

  return (
    <div className="lib-page">
      <Header />

      <main className="lib-main">
        <div className="lib-header">
          <div className="lib-header-text">
            <span className="section-badge">Your Collection</span>
            <h2 className="section-title">My Wildlife Library</h2>
            <p className="section-subtitle">
              Animals you've encountered, all in one place. Tap a card to see
              the details, or switch to the map to see where they were.
            </p>
          </div>

          <div className="lib-tabs">
            <button
              className={`lib-tab ${tab === 'collection' ? 'active' : ''}`}
              onClick={() => setTab('collection')}
            >
              <svg
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <rect x="3" y="3" width="7" height="7" />
                <rect x="14" y="3" width="7" height="7" />
                <rect x="14" y="14" width="7" height="7" />
                <rect x="3" y="14" width="7" height="7" />
              </svg>
              Collection
              <span className="lib-tab-count">{sightings.length}</span>
            </button>
            <button
              className={`lib-tab ${tab === 'map' ? 'active' : ''}`}
              onClick={() => setTab('map')}
            >
              <svg
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
                <circle cx="12" cy="10" r="3" />
              </svg>
              Map
            </button>
          </div>
        </div>

        {error && (
          <div className="lib-error">
            <span>{error}</span>
            <button onClick={() => setError('')}>×</button>
          </div>
        )}

        {tab === 'collection' && (
          <section className="lib-collection">
            {loading ? (
              <div className="lib-empty">
                <div className="geo-loading-spinner"></div>
                <span>Loading your collection…</span>
              </div>
            ) : sightings.length === 0 ? (
              <div className="lib-empty">
                <div className="lib-empty-icon">🦁</div>
                <h3>No animals saved yet</h3>
                <p>
                  Head over to the AI Assistant, recognise an animal, then tap{' '}
                  <strong>Save to Library</strong> to start your collection.
                </p>
                <button
                  className="btn btn-primary"
                  onClick={() => navigate('/ai')}
                >
                  Go to AI Assistant
                </button>
              </div>
            ) : (
              <div className="lib-cards-grid">
                {sightings.map((s) => (
                  <FlipCard key={s.id} sighting={s} onDelete={onDelete} />
                ))}
              </div>
            )}
          </section>
        )}

        {tab === 'map' && (
          <section className="lib-map-section">
            <div className="lib-radius-bar">
              <div className="lib-mode-toggle" role="tablist">
                <button
                  role="tab"
                  aria-selected={mapMode === 'all'}
                  className={`lib-mode-btn ${mapMode === 'all' ? 'active' : ''}`}
                  onClick={() => setMapMode('all')}
                >
                  <svg
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                  >
                    <circle cx="12" cy="12" r="10" />
                  </svg>
                  All my sightings
                </button>
                <button
                  role="tab"
                  aria-selected={mapMode === 'radius'}
                  className={`lib-mode-btn ${mapMode === 'radius' ? 'active' : ''}`}
                  onClick={() => setMapMode('radius')}
                >
                  <svg
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                  >
                    <circle cx="12" cy="12" r="3" />
                    <circle cx="12" cy="12" r="9" strokeDasharray="3 3" />
                  </svg>
                  Within radius
                </button>
              </div>

              {mapMode === 'radius' && (
                <div className="lib-radius-field">
                  <div className="lib-radius-input">
                    <input
                      type="number"
                      min="1"
                      max="1000"
                      step="1"
                      placeholder="km"
                      value={radiusInput}
                      onChange={(e) => {
                        const raw = e.target.value;
                        setRadiusInput(raw);
                        const n = parseFloat(raw);
                        setRadiusKm(
                          Number.isFinite(n) && n > 0 ? Math.min(n, 1000) : 0
                        );
                      }}
                    />
                    <span className="lib-radius-unit">km of me</span>
                  </div>
                  <span className="lib-radius-help">
                    Max 1000 km — for the whole world use “All my sightings”.
                  </span>
                </div>
              )}

              <div className="lib-radius-status">
                {mapMode === 'radius' && !userLocation && (
                  <button className="btn btn-outline" onClick={requestLocation}>
                    <svg
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                      style={{ width: 16, height: 16 }}
                    >
                      <circle cx="12" cy="12" r="10" />
                      <circle cx="12" cy="12" r="3" />
                    </svg>
                    Enable my location
                  </button>
                )}
                {mapMode === 'all' && (
                  <div className="lib-radius-pill">
                    Showing all {sightings.length} sightings
                  </div>
                )}
                {mapMode === 'radius' && userLocation && radiusActive && (
                  <div className="lib-radius-pill active">
                    {visibleOnMap.length} within {radiusKm} km
                  </div>
                )}
                {nearbyLoading && <span className="ai-spinner small"></span>}
              </div>
            </div>

            <div className="lib-map-container">
              <MapContainer
                center={[20, 0]}
                zoom={2}
                style={{ height: '100%', width: '100%' }}
              >
                <TileLayer
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                />

                <MapAutoFit
                  markers={visibleOnMap}
                  center={
                    userLocation ? [userLocation.lat, userLocation.lng] : null
                  }
                />

                {userLocation && (
                  <Marker
                    position={[userLocation.lat, userLocation.lng]}
                    icon={userLocationIcon}
                  >
                    <Popup>You are here</Popup>
                  </Marker>
                )}

                {userLocation && radiusActive && (
                  <Circle
                    center={[userLocation.lat, userLocation.lng]}
                    radius={radiusKm * 1000}
                    pathOptions={{
                      color: '#16a34a',
                      fillColor: '#16a34a',
                      fillOpacity: 0.08,
                      weight: 2,
                      dashArray: '6, 6',
                    }}
                  />
                )}

                {visibleOnMap.map((s) => (
                  <Marker
                    key={s.id}
                    position={[s.latitude, s.longitude]}
                    icon={createAnimalMarker(s.commonName, categoryOf(s))}
                  >
                    <Popup className="geo-marker-popup">
                      <div className="geo-popup-content">
                        <span
                          className={`geo-popup-category category-${categoryOf(s)}`}
                        >
                          {categoryLabel(s)}
                        </span>
                        <h4>{s.commonName}</h4>
                        <p
                          style={{
                            fontStyle: 'italic',
                            margin: 0,
                            fontSize: 12,
                          }}
                        >
                          {s.scientificName}
                        </p>
                        <p style={{ margin: '6px 0 0', fontSize: 12 }}>
                          Sighted {formatDate(s.sightedAt)}
                          {typeof s.distanceKm === 'number' &&
                            ` · ${s.distanceKm.toFixed(1)} km away`}
                        </p>
                      </div>
                    </Popup>
                  </Marker>
                ))}
              </MapContainer>
            </div>
          </section>
        )}
      </main>

      <Footer />
    </div>
  );
}
