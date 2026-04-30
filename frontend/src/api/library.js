import { api } from "./client";

// Save a recognized animal to the current user's library.
export async function createSighting(payload) {
  const res = await api.post("/api/library/sightings", payload);
  return res.data;
}

// Fetch every sighting belonging to the authenticated user.
export async function getMyLibrary() {
  const res = await api.get("/api/library/sightings");
  return res.data;
}

// Delete one of the current user's sightings.
export async function deleteSighting(id) {
  await api.delete(`/api/library/sightings/${id}`);
}

// Calls fn_user_nearby_sightings server-side and returns sightings within
// `radiusKm` of (lat, lng), sorted by distance.
export async function getNearbySightings(lat, lng, radiusKm) {
  const res = await api.get("/api/library/nearby", {
    params: { lat, lng, radiusKm },
  });
  return res.data;
}

// Resolve a recognized common name (e.g. "Lion") to a Species record.
// Returns null when not found instead of throwing — the modal uses that
// to show a friendly "we couldn't match this species" hint.
export async function getSpeciesByName(name) {
  try {
    const res = await api.get("/api/species/by-name", {
      params: { name },
    });
    return res.data;
  } catch (err) {
    if (err.response?.status === 404) return null;
    throw err;
  }
}
