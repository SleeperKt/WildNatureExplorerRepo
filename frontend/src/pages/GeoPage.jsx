import { useEffect, useState, useRef, useCallback } from 'react';
import { api } from '../api/client';
import {
  MapContainer,
  TileLayer,
  Marker,
  Popup,
  Circle,
  Polyline,
  useMap,
  useMapEvents,
} from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import Header from '../components/Header';
import Footer from '../components/Footer';

// Import SVG icons
import dangerousIcon from '../images/dangerous.svg';
import rareIcon from '../images/rare.svg';
import commonIcon from '../images/common.svg';

// Create custom div icon with colored circle and animal name
const createAnimalMarker = (name, category) => {
  const colors = {
    dangerous: { bg: '#ef4444', border: '#dc2626' },
    rare: { bg: '#8b5cf6', border: '#7c3aed' },
    common: { bg: '#22c55e', border: '#16a34a' },
  };
  const color = colors[category] || colors.common;

  // Truncate name if too long
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

// User location icon
const userLocationIcon = L.divIcon({
  className: 'user-location-marker',
  html: `<div class="user-marker-dot"></div><div class="user-marker-pulse"></div>`,
  iconSize: [24, 24],
  iconAnchor: [12, 12],
});

// Path point marker
const createPathPointMarker = (index, isActive) => {
  return L.divIcon({
    className: 'path-point-marker',
    html: `
      <div class="path-point ${isActive ? 'active' : ''}" style="
        width: 28px;
        height: 28px;
        background: ${isActive ? '#3b82f6' : '#6b7280'};
        border: 3px solid white;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        color: white;
        font-weight: bold;
        font-size: 12px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.3);
      ">${index + 1}</div>
    `,
    iconSize: [28, 28],
    iconAnchor: [14, 14],
  });
};

// Simulated user marker (blue dot with animation)
const simulatedUserIcon = L.divIcon({
  className: 'simulated-user-marker',
  html: `<div class="simulated-marker-pulse"></div><div class="simulated-marker-dot"></div>`,
  iconSize: [40, 40],
  iconAnchor: [20, 20],
});

// Haversine distance calculation
const calculateDistance = (lat1, lon1, lat2, lon2) => {
  const R = 6371; // Earth radius in km
  const dLat = ((lat2 - lat1) * Math.PI) / 180;
  const dLon = ((lon2 - lon1) * Math.PI) / 180;
  const a =
    Math.sin(dLat / 2) * Math.sin(dLat / 2) +
    Math.cos((lat1 * Math.PI) / 180) *
      Math.cos((lat2 * Math.PI) / 180) *
      Math.sin(dLon / 2) *
      Math.sin(dLon / 2);
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  return R * c;
};

// Calculate bearing (direction) between two points
const calculateBearing = (lat1, lon1, lat2, lon2) => {
  const dLon = ((lon2 - lon1) * Math.PI) / 180;
  const lat1Rad = (lat1 * Math.PI) / 180;
  const lat2Rad = (lat2 * Math.PI) / 180;

  const y = Math.sin(dLon) * Math.cos(lat2Rad);
  const x =
    Math.cos(lat1Rad) * Math.sin(lat2Rad) -
    Math.sin(lat1Rad) * Math.cos(lat2Rad) * Math.cos(dLon);

  let bearing = (Math.atan2(y, x) * 180) / Math.PI;
  bearing = (bearing + 360) % 360; // Normalize to 0-360
  return bearing;
};

// Convert bearing to compass direction
const getCompassDirection = (bearing) => {
  const directions = [
    'N',
    'NNE',
    'NE',
    'ENE',
    'E',
    'ESE',
    'SE',
    'SSE',
    'S',
    'SSW',
    'SW',
    'WSW',
    'W',
    'WNW',
    'NW',
    'NNW',
  ];
  const index = Math.round(bearing / 22.5) % 16;
  return directions[index];
};

// Interpolate between two points
const interpolate = (start, end, fraction) => {
  return {
    lat: start.lat + (end.lat - start.lat) * fraction,
    lng: start.lng + (end.lng - start.lng) * fraction,
  };
};

// Path click handler component
function PathClickHandler({ onMapClick, isBuilding }) {
  useMapEvents({
    click: (e) => {
      if (isBuilding) {
        onMapClick(e.latlng);
      }
    },
  });
  return null;
}

// Component to handle map bounds changes
function MapController({ bounds, center, zoom }) {
  const map = useMap();

  useEffect(() => {
    if (bounds) {
      map.fitBounds(bounds, { padding: [50, 50] });
    } else if (center) {
      map.setView(center, zoom || 6);
    }
  }, [map, bounds, center, zoom]);

  return null;
}

export default function GeoPage() {
  // State
  const [countries, setCountries] = useState([]);
  const [selectedCountry, setSelectedCountry] = useState('');
  const [countryName, setCountryName] = useState('');
  const [points, setPoints] = useState([]);
  const [filteredPoints, setFilteredPoints] = useState([]);
  const [selectedPoint, setSelectedPoint] = useState(null);
  const [loading, setLoading] = useState(false);
  const [mapBounds, setMapBounds] = useState(null);

  // Filters
  const [activeFilter, setActiveFilter] = useState('all'); // all, dangerous, rare
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState(null);

  // Danger mode
  const [dangerMode, setDangerMode] = useState(false);
  const [dangerPoints, setDangerPoints] = useState([]);

  // User location tracking
  const [userLocation, setUserLocation] = useState(null);
  const [locationEnabled, setLocationEnabled] = useState(false);
  const [userPath, setUserPath] = useState([]);
  const [, setProximityAlerts] = useState([]);
  const [showNotification, setShowNotification] = useState(false);
  const [notificationMessage, setNotificationMessage] = useState('');
  const [notificationType, setNotificationType] = useState('info'); // info, warning, danger, success

  // Path builder state
  const [pathPoints, setPathPoints] = useState([]);
  const [isBuilding, setIsBuilding] = useState(false);
  const [isSimulating, setIsSimulating] = useState(false);
  const [simulatedPosition, setSimulatedPosition] = useState(null);
  const [traveledPath, setTraveledPath] = useState([]);
  const [currentPathIndex, setCurrentPathIndex] = useState(0);
  const [visitedZones, setVisitedZones] = useState(new Set());

  const locationIntervalRef = useRef(null);
  const notificationTimeoutRef = useRef(null);
  const simulationRef = useRef(null);

  // Fetch countries on mount
  useEffect(() => {
    api.get('/api/reference/countries').then((res) => setCountries(res.data));

    return () => {
      if (locationIntervalRef.current) {
        clearInterval(locationIntervalRef.current);
      }
      if (notificationTimeoutRef.current) {
        clearTimeout(notificationTimeoutRef.current);
      }
      if (simulationRef.current) {
        clearInterval(simulationRef.current);
      }
    };
  }, []);

  // Apply filters when points or filter changes
  useEffect(() => {
    if (!points.length) {
      setFilteredPoints([]);
      return;
    }

    let filtered = [...points];

    if (activeFilter === 'dangerous') {
      filtered = filtered.filter((p) => p.isDangerous);
    } else if (activeFilter === 'rare') {
      filtered = filtered.filter((p) => p.isRare);
    }

    setFilteredPoints(filtered);
  }, [points, activeFilter]);

  // Load country data
  const loadCountry = async (id) => {
    if (!id) {
      setPoints([]);
      setFilteredPoints([]);
      setSelectedCountry('');
      setCountryName('');
      setMapBounds(null);
      setDangerPoints([]);
      return;
    }

    try {
      setLoading(true);
      setSelectedCountry(id);
      setSearchResults(null);
      setSearchQuery('');

      // Get country bounds
      const boundsRes = await api.get(`/api/map/country/${id}/bounds`);
      if (boundsRes.data.hasData) {
        setCountryName(boundsRes.data.countryName);
        const b = boundsRes.data.bounds;
        setMapBounds([
          [b.south, b.west],
          [b.north, b.east],
        ]);
      }

      // Get species points
      const res = await api.get(`/api/map/country/${id}`);
      setPoints(res.data);

      // Get dangerous points for danger mode
      const dangerRes = await api.get(`/api/map/country/${id}/dangerous`);
      setDangerPoints(dangerRes.data);

      setSelectedPoint(null);
    } catch (error) {
      console.error('Error loading country:', error);
      setPoints([]);
      setMapBounds(null);
    } finally {
      setLoading(false);
    }
  };

  // Search for animal in country
  const handleSearch = async () => {
    if (!selectedCountry || !searchQuery.trim()) return;

    try {
      setLoading(true);
      const res = await api.get(
        `/api/map/country/${selectedCountry}/search?query=${encodeURIComponent(searchQuery)}`
      );
      setSearchResults(res.data);

      if (res.data.found && res.data.results.length > 0) {
        setFilteredPoints(res.data.results);
      }
    } catch (error) {
      console.error('Search error:', error);
    } finally {
      setLoading(false);
    }
  };

  // Clear search
  const clearSearch = () => {
    setSearchQuery('');
    setSearchResults(null);
    setFilteredPoints(
      points.filter((p) => {
        if (activeFilter === 'dangerous') return p.isDangerous;
        if (activeFilter === 'rare') return p.isRare;
        return true;
      })
    );
  };

  // Get icon for point
  const getIconForPoint = (point) => {
    const category = point.isDangerous
      ? 'dangerous'
      : point.isRare
        ? 'rare'
        : 'common';
    return createAnimalMarker(point.commonName, category);
  };

  // Get category label
  const getCategoryLabel = (point) => {
    if (point.isDangerous) return 'Dangerous';
    if (point.isRare) return 'Rare';
    return 'Common';
  };

  // Get category class
  const getCategoryClass = (point) => {
    if (point.isDangerous) return 'category-dangerous';
    if (point.isRare) return 'category-rare';
    return 'category-common';
  };

  // Show notification with type
  const showNotificationMessage = useCallback((message, type = 'info') => {
    setNotificationMessage(message);
    setNotificationType(type);
    setShowNotification(true);

    if (notificationTimeoutRef.current) {
      clearTimeout(notificationTimeoutRef.current);
    }

    notificationTimeoutRef.current = setTimeout(() => {
      setShowNotification(false);
    }, 5000);
  }, []);

  // Path builder functions
  const handlePathClick = (latlng) => {
    if (pathPoints.length < 5) {
      setPathPoints((prev) => [...prev, latlng]);
      if (pathPoints.length === 4) {
        showNotificationMessage(
          "Path complete! Click 'Run Simulation' to test",
          'success'
        );
      }
    }
  };

  const startBuildingPath = () => {
    setIsBuilding(true);
    setPathPoints([]);
    setSimulatedPosition(null);
    setTraveledPath([]);
    setVisitedZones(new Set());
    setIsSimulating(false);
    showNotificationMessage(
      'Click on the map to place up to 5 path points',
      'info'
    );
  };

  const clearPath = () => {
    setPathPoints([]);
    setIsBuilding(false);
    setSimulatedPosition(null);
    setTraveledPath([]);
    setIsSimulating(false);
    setCurrentPathIndex(0);
    setVisitedZones(new Set());
    if (simulationRef.current) {
      clearInterval(simulationRef.current);
    }
  };

  // Run path simulation (client-side only)
  const runSimulation = useCallback(() => {
    if (pathPoints.length < 2) {
      showNotificationMessage('Add at least 2 points to simulate', 'warning');
      return;
    }

    setIsSimulating(true);
    setIsBuilding(false);
    setCurrentPathIndex(0);
    setVisitedZones(new Set());
    setTraveledPath([]);

    // Generate path steps client-side by interpolating between waypoints
    const pathSteps = [];
    const stepsPerSegment = 15;

    for (let i = 0; i < pathPoints.length - 1; i++) {
      for (let step = 0; step < stepsPerSegment; step++) {
        const fraction = step / stepsPerSegment;
        const position = interpolate(
          pathPoints[i],
          pathPoints[i + 1],
          fraction
        );
        pathSteps.push({
          latitude: position.lat,
          longitude: position.lng,
          segmentId: i,
          stepId: i * stepsPerSegment + step,
        });
      }
    }

    showNotificationMessage('Starting client-side simulation...', 'info');

    // Animate along the path
    let stepIndex = 0;

    simulationRef.current = setInterval(() => {
      if (stepIndex >= pathSteps.length) {
        clearInterval(simulationRef.current);
        setIsSimulating(false);
        showNotificationMessage('✅ Simulation completed!', 'success');
        return;
      }

      const step = pathSteps[stepIndex];
      const position = { lat: step.latitude, lng: step.longitude };

      setSimulatedPosition(position);
      setTraveledPath((prev) => [...prev, position]);
      setCurrentPathIndex(step.segmentId);

      // Check proximity to danger animals client-side
      dangerPoints.forEach((animal) => {
        const distance = calculateDistance(
          position.lat,
          position.lng,
          animal.latitude,
          animal.longitude
        );

        const bearing = calculateBearing(
          position.lat,
          position.lng,
          animal.latitude,
          animal.longitude
        );
        const direction = getCompassDirection(bearing);

        const dangerRadius = 10; // 10km
        const warningRadius = 15; // 15km warning
        const zoneKey = `${animal.id || animal.commonName}`;

        if (
          distance <= dangerRadius &&
          !visitedZones.has(`${zoneKey}-danger`)
        ) {
          showNotificationMessage(
            `⚠️ DANGER! You entered the danger zone of ${animal.commonName}! Distance: ${distance.toFixed(1)}km to the ${direction}`,
            'danger'
          );
          setVisitedZones((prev) => new Set([...prev, `${zoneKey}-danger`]));
        } else if (
          distance <= warningRadius &&
          distance > dangerRadius &&
          !visitedZones.has(`${zoneKey}-warning`)
        ) {
          showNotificationMessage(
            `⚡ Warning! Approaching ${animal.commonName} danger zone - ${distance.toFixed(1)}km away to the ${direction}`,
            'warning'
          );
          setVisitedZones((prev) => new Set([...prev, `${zoneKey}-warning`]));
        }
      });

      stepIndex++;
    }, 400);
  }, [pathPoints, dangerPoints, visitedZones, showNotificationMessage]);

  const stopSimulation = () => {
    if (simulationRef.current) {
      clearInterval(simulationRef.current);
    }
    setIsSimulating(false);
    showNotificationMessage('Simulation stopped', 'info');
  };

  // Check proximity to dangerous animals
  const checkProximity = useCallback(
    async (location) => {
      if (!selectedCountry) return;

      try {
        const res = await api.post('/api/map/proximity-check', {
          countryId: selectedCountry,
          userLatitude: location.lat,
          userLongitude: location.lng,
        });

        setProximityAlerts(res.data.alerts);

        if (res.data.alertCount > 0) {
          const criticalAlerts = res.data.alerts.filter(
            (a) => a.warning === 'CRITICAL'
          );
          if (criticalAlerts.length > 0) {
            const bearing = calculateBearing(
              location.lat,
              location.lng,
              criticalAlerts[0].latitude,
              criticalAlerts[0].longitude
            );
            const direction = getCompassDirection(bearing);
            showNotificationMessage(
              `⚠️ DANGER: You are within 5km of ${criticalAlerts[0].commonName} to the ${direction}!`
            );
          } else {
            showNotificationMessage(
              `Warning: ${res.data.alertCount} dangerous animal(s) nearby`
            );
          }
        }
      } catch (error) {
        console.error('Proximity check error:', error);
      }
    },
    [selectedCountry, showNotificationMessage]
  );

  // Enable user location tracking
  const enableLocationTracking = useCallback(() => {
    if (!navigator.geolocation) {
      showNotificationMessage('Geolocation is not supported by your browser');
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        const loc = {
          lat: position.coords.latitude,
          lng: position.coords.longitude,
          timestamp: new Date().toISOString(),
        };
        setUserLocation(loc);
        setUserPath([loc]);
        setLocationEnabled(true);

        // Check proximity immediately
        if (dangerMode && selectedCountry) {
          checkProximity(loc);
        }

        // Start interval to update location every 15 minutes
        locationIntervalRef.current = setInterval(
          () => {
            navigator.geolocation.getCurrentPosition(
              (pos) => {
                const newLoc = {
                  lat: pos.coords.latitude,
                  lng: pos.coords.longitude,
                  timestamp: new Date().toISOString(),
                };
                setUserLocation(newLoc);
                setUserPath((prev) => [...prev, newLoc]);

                // Check proximity if in danger mode
                if (dangerMode && selectedCountry) {
                  checkProximity(newLoc);
                }
              },
              (err) => console.error('Location error:', err),
              { enableHighAccuracy: true }
            );
          },
          15 * 60 * 1000
        ); // 15 minutes
      },
      (error) => {
        console.error('Location error:', error);
        showNotificationMessage(
          'Please allow location access to use this feature'
        );
      },
      { enableHighAccuracy: true }
    );
  }, [dangerMode, selectedCountry, checkProximity, showNotificationMessage]);

  // Disable location tracking
  const disableLocationTracking = () => {
    if (locationIntervalRef.current) {
      clearInterval(locationIntervalRef.current);
    }
    setLocationEnabled(false);
  };

  // Toggle danger mode
  const toggleDangerMode = () => {
    if (!selectedCountry) {
      showNotificationMessage('Please select a country first');
      return;
    }
    setDangerMode(!dangerMode);
  };

  // Get points to display based on mode
  const displayPoints = dangerMode ? dangerPoints : filteredPoints;

  return (
    <div className="geo-page">
      <Header />

      {/* Notification */}
      {showNotification && (
        <div className={`geo-notification notification-${notificationType}`}>
          <span>{notificationMessage}</span>
          <button onClick={() => setShowNotification(false)}>×</button>
        </div>
      )}

      <main className="geo-main">
        <div className="geo-layout">
          {/* Sidebar */}
          <aside className="geo-sidebar">
            <div className="geo-sidebar-header">
              <h1>Wildlife Map</h1>
              <p>Explore animals by location</p>
            </div>

            {/* Country Selector */}
            <div className="geo-control-group">
              <label>Select Country</label>
              <select
                value={selectedCountry}
                onChange={(e) => loadCountry(e.target.value)}
                disabled={loading}
                className="geo-select"
              >
                <option value="">Choose a country...</option>
                {countries.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </select>
            </div>

            {/* Search (only after country is selected) */}
            {selectedCountry && (
              <div className="geo-control-group">
                <label>Search Animal</label>
                <div className="geo-search-input">
                  <input
                    type="text"
                    placeholder="Enter animal name..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                  />
                  {searchQuery && (
                    <button className="geo-search-clear" onClick={clearSearch}>
                      ×
                    </button>
                  )}
                  <button
                    className="geo-search-btn"
                    onClick={handleSearch}
                    disabled={!searchQuery.trim()}
                  >
                    <svg
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <circle cx="11" cy="11" r="8" />
                      <path d="m21 21-4.35-4.35" />
                    </svg>
                  </button>
                </div>
                {searchResults && (
                  <div
                    className={`geo-search-result ${searchResults.found ? 'found' : 'not-found'}`}
                  >
                    {searchResults.found
                      ? `Found ${searchResults.count} location(s) for "${searchResults.query}"`
                      : `"${searchResults.query}" not found in this country`}
                  </div>
                )}
              </div>
            )}

            {/* Filters (only after country is selected) */}
            {selectedCountry && !dangerMode && (
              <div className="geo-control-group">
                <label>Filter by Category</label>
                <div className="geo-filter-buttons">
                  <button
                    className={`geo-filter-btn ${activeFilter === 'all' ? 'active' : ''}`}
                    onClick={() => setActiveFilter('all')}
                  >
                    All
                  </button>
                  <button
                    className={`geo-filter-btn filter-dangerous ${activeFilter === 'dangerous' ? 'active' : ''}`}
                    onClick={() => setActiveFilter('dangerous')}
                  >
                    Dangerous
                  </button>
                  <button
                    className={`geo-filter-btn filter-rare ${activeFilter === 'rare' ? 'active' : ''}`}
                    onClick={() => setActiveFilter('rare')}
                  >
                    Rare
                  </button>
                </div>
              </div>
            )}

            {/* Danger Mode Toggle */}
            {selectedCountry && (
              <div className="geo-control-group">
                <label>Danger Alert System</label>
                <button
                  className={`geo-danger-toggle ${dangerMode ? 'active' : ''}`}
                  onClick={toggleDangerMode}
                >
                  <svg
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                  >
                    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
                    <line x1="12" y1="9" x2="12" y2="13" />
                    <line x1="12" y1="17" x2="12.01" y2="17" />
                  </svg>
                  {dangerMode ? 'Exit Danger Mode' : 'Enter Danger Mode'}
                </button>
              </div>
            )}

            {/* Location Tracking (only in danger mode) */}
            {dangerMode && (
              <div className="geo-control-group">
                <label>Your Location</label>
                <button
                  className={`geo-location-toggle ${locationEnabled ? 'active' : ''}`}
                  onClick={
                    locationEnabled
                      ? disableLocationTracking
                      : enableLocationTracking
                  }
                >
                  <svg
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                  >
                    <circle cx="12" cy="12" r="10" />
                    <circle cx="12" cy="12" r="3" />
                    <line x1="12" y1="2" x2="12" y2="4" />
                    <line x1="12" y1="20" x2="12" y2="22" />
                    <line x1="2" y1="12" x2="4" y2="12" />
                    <line x1="20" y1="12" x2="22" y2="12" />
                  </svg>
                  {locationEnabled ? 'Tracking Active' : 'Enable Location'}
                </button>
                {locationEnabled && userLocation && (
                  <div className="geo-location-info">
                    <p>Lat: {userLocation.lat.toFixed(4)}</p>
                    <p>Lng: {userLocation.lng.toFixed(4)}</p>
                    <p className="geo-location-note">
                      Updates every 15 minutes
                    </p>
                  </div>
                )}
              </div>
            )}

            {/* Path Simulator (in danger mode) */}
            {dangerMode && (
              <div className="geo-control-group path-simulator">
                <label>
                  <svg
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                    style={{
                      width: 16,
                      height: 16,
                      marginRight: 6,
                      verticalAlign: 'middle',
                    }}
                  >
                    <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
                    <circle cx="12" cy="10" r="3" />
                  </svg>
                  Path Simulator
                </label>
                <p className="geo-helper-text">
                  Build a test path to check danger zone notifications
                </p>

                {!isBuilding && !isSimulating && pathPoints.length === 0 && (
                  <button
                    className="geo-path-btn start"
                    onClick={startBuildingPath}
                  >
                    <svg
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <circle cx="12" cy="12" r="10" />
                      <line x1="12" y1="8" x2="12" y2="16" />
                      <line x1="8" y1="12" x2="16" y2="12" />
                    </svg>
                    Start Building Path
                  </button>
                )}

                {isBuilding && (
                  <div className="path-builder-status">
                    <div className="path-progress">
                      <span className="path-count">{pathPoints.length}/5</span>
                      <span>points placed</span>
                    </div>
                    <p className="path-instruction">
                      Click on the map to add waypoints
                    </p>
                    <div className="path-actions">
                      {pathPoints.length >= 2 && (
                        <button
                          className="geo-path-btn run"
                          onClick={runSimulation}
                        >
                          <svg viewBox="0 0 24 24" fill="currentColor">
                            <path d="M8 5v14l11-7z" />
                          </svg>
                          Run
                        </button>
                      )}
                      <button
                        className="geo-path-btn clear"
                        onClick={clearPath}
                      >
                        Clear
                      </button>
                    </div>
                  </div>
                )}

                {pathPoints.length > 0 && !isBuilding && !isSimulating && (
                  <div className="path-actions">
                    <button
                      className="geo-path-btn run"
                      onClick={runSimulation}
                    >
                      <svg viewBox="0 0 24 24" fill="currentColor">
                        <path d="M8 5v14l11-7z" />
                      </svg>
                      Run Simulation
                    </button>
                    <button
                      className="geo-path-btn edit"
                      onClick={startBuildingPath}
                    >
                      Edit
                    </button>
                    <button className="geo-path-btn clear" onClick={clearPath}>
                      Clear
                    </button>
                  </div>
                )}

                {isSimulating && (
                  <div className="simulation-active">
                    <div className="simulation-indicator">
                      <span className="pulse-dot"></span>
                      <span>Simulating movement...</span>
                    </div>
                    <button
                      className="geo-path-btn stop"
                      onClick={stopSimulation}
                    >
                      <svg viewBox="0 0 24 24" fill="currentColor">
                        <rect x="6" y="6" width="12" height="12" />
                      </svg>
                      Stop
                    </button>
                  </div>
                )}
              </div>
            )}

            {/* Map Legend */}
            <div className="geo-legend">
              <h3>Map Legend</h3>
              <div className="geo-legend-items">
                <div className="geo-legend-item">
                  <img src={dangerousIcon} alt="Dangerous" />
                  <span>Dangerous Animal</span>
                </div>
                <div className="geo-legend-item">
                  <img src={rareIcon} alt="Rare" />
                  <span>Rare Animal</span>
                </div>
                <div className="geo-legend-item">
                  <img src={commonIcon} alt="Common" />
                  <span>Common Animal</span>
                </div>
                {dangerMode && (
                  <>
                    <div className="geo-legend-item">
                      <div className="legend-circle danger-radius"></div>
                      <span>10km Danger Zone</span>
                    </div>
                    <div className="geo-legend-item">
                      <div className="legend-circle user-location"></div>
                      <span>Your Location</span>
                    </div>
                    {(isBuilding || pathPoints.length > 0) && (
                      <>
                        <div className="geo-legend-item">
                          <div className="legend-circle path-point"></div>
                          <span>Path Waypoint</span>
                        </div>
                        <div className="geo-legend-item">
                          <div className="legend-circle simulated-user"></div>
                          <span>Simulated User</span>
                        </div>
                      </>
                    )}
                  </>
                )}
              </div>
            </div>

            {/* Stats */}
            {selectedCountry && (
              <div className="geo-stats">
                <div className="geo-stat">
                  <span className="geo-stat-value">{displayPoints.length}</span>
                  <span className="geo-stat-label">Locations</span>
                </div>
                {!dangerMode && (
                  <>
                    <div className="geo-stat dangerous">
                      <span className="geo-stat-value">
                        {points.filter((p) => p.isDangerous).length}
                      </span>
                      <span className="geo-stat-label">Dangerous</span>
                    </div>
                    <div className="geo-stat rare">
                      <span className="geo-stat-value">
                        {points.filter((p) => p.isRare).length}
                      </span>
                      <span className="geo-stat-label">Rare</span>
                    </div>
                  </>
                )}
              </div>
            )}
          </aside>

          {/* Map Area */}
          <div className="geo-map-container">
            {loading && (
              <div className="geo-loading">
                <div className="geo-loading-spinner"></div>
                <span>Loading wildlife data...</span>
              </div>
            )}

            <MapContainer
              center={[20, 0]}
              zoom={2}
              className="geo-map-full"
              style={{ height: '100%', width: '100%' }}
            >
              {/* OpenStreetMap with terrain details */}
              <TileLayer
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
              />

              <MapController bounds={mapBounds} />

              {/* Path click handler */}
              <PathClickHandler
                onMapClick={handlePathClick}
                isBuilding={isBuilding}
              />

              {/* User path in danger mode */}
              {dangerMode && userPath.length > 1 && (
                <Polyline
                  positions={userPath.map((p) => [p.lat, p.lng])}
                  color="#3b82f6"
                  weight={3}
                  opacity={0.7}
                  dashArray="10, 10"
                />
              )}

              {/* Danger zone circles */}
              {dangerMode &&
                dangerPoints.map((point, i) => (
                  <Circle
                    key={`danger-circle-${i}`}
                    center={[point.latitude, point.longitude]}
                    radius={10000} // 10km in meters
                    pathOptions={{
                      color: '#ef4444',
                      fillColor: '#ef4444',
                      fillOpacity: 0.15,
                      weight: 2,
                      dashArray: '5, 5',
                    }}
                  />
                ))}

              {/* Animal markers */}
              {displayPoints.map((point, i) => (
                <Marker
                  key={`marker-${i}`}
                  position={[point.latitude, point.longitude]}
                  icon={getIconForPoint(point)}
                  eventHandlers={{
                    click: () => setSelectedPoint(point),
                  }}
                >
                  <Popup className="geo-marker-popup">
                    <div className="geo-popup-content">
                      <span
                        className={`geo-popup-category ${getCategoryClass(point)}`}
                      >
                        {getCategoryLabel(point)}
                      </span>
                      <h4>{point.commonName}</h4>
                    </div>
                  </Popup>
                </Marker>
              ))}

              {/* User location marker */}
              {dangerMode && userLocation && (
                <Marker
                  position={[userLocation.lat, userLocation.lng]}
                  icon={userLocationIcon}
                >
                  <Popup>Your Location</Popup>
                </Marker>
              )}

              {/* Path waypoints */}
              {pathPoints.map((point, index) => (
                <Marker
                  key={`path-point-${index}`}
                  position={[point.lat, point.lng]}
                  icon={createPathPointMarker(
                    index,
                    currentPathIndex === index && isSimulating
                  )}
                />
              ))}

              {/* Path line (planned route) */}
              {pathPoints.length > 1 && (
                <Polyline
                  positions={pathPoints.map((p) => [p.lat, p.lng])}
                  pathOptions={{
                    color: '#6b7280',
                    weight: 3,
                    dashArray: '8, 8',
                    opacity: 0.7,
                  }}
                />
              )}

              {/* Traveled path (simulated) */}
              {traveledPath.length > 1 && (
                <Polyline
                  positions={traveledPath.map((p) => [p.lat, p.lng])}
                  pathOptions={{
                    color: '#22c55e',
                    weight: 4,
                    opacity: 0.9,
                  }}
                />
              )}

              {/* Simulated user position */}
              {simulatedPosition && (
                <Marker
                  position={[simulatedPosition.lat, simulatedPosition.lng]}
                  icon={simulatedUserIcon}
                >
                  <Popup>Simulated Position</Popup>
                </Marker>
              )}
            </MapContainer>

            {/* Country label */}
            {countryName && (
              <div className="geo-country-label">
                {countryName}
                {dangerMode && (
                  <span className="danger-badge">DANGER MODE</span>
                )}
                {isSimulating && (
                  <span className="simulation-badge">SIMULATING</span>
                )}
              </div>
            )}
          </div>

          {/* Info Panel */}
          {selectedPoint && (
            <aside className="geo-info-panel">
              <button
                className="geo-panel-close"
                onClick={() => setSelectedPoint(null)}
              >
                ×
              </button>

              <div className="geo-panel-header">
                <span
                  className={`geo-panel-category ${getCategoryClass(selectedPoint)}`}
                >
                  {getCategoryLabel(selectedPoint)}
                </span>
                <img
                  src={
                    selectedPoint.isDangerous
                      ? dangerousIcon
                      : selectedPoint.isRare
                        ? rareIcon
                        : commonIcon
                  }
                  alt=""
                  className="geo-panel-icon"
                />
              </div>

              <h2 className="geo-panel-title">{selectedPoint.commonName}</h2>
              <p className="geo-panel-scientific">
                {selectedPoint.scientificName}
              </p>

              <div className="geo-panel-description">
                <p>{selectedPoint.description}</p>
              </div>

              <div className="geo-panel-location">
                <h4>Location</h4>
                <p>
                  <strong>Latitude:</strong> {selectedPoint.latitude.toFixed(4)}
                </p>
                <p>
                  <strong>Longitude:</strong>{' '}
                  {selectedPoint.longitude.toFixed(4)}
                </p>
                {selectedPoint.locationDescription && (
                  <p>
                    <strong>Details:</strong>{' '}
                    {selectedPoint.locationDescription}
                  </p>
                )}
              </div>

              {selectedPoint.isDangerous && (
                <div className="geo-panel-warning">
                  <svg
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                  >
                    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
                    <line x1="12" y1="9" x2="12" y2="13" />
                    <line x1="12" y1="17" x2="12.01" y2="17" />
                  </svg>
                  <span>This animal can be dangerous. Keep safe distance!</span>
                </div>
              )}
            </aside>
          )}
        </div>
      </main>

      <Footer />
    </div>
  );
}
