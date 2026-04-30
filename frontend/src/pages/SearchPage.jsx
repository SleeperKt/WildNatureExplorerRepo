import { useEffect, useState, useCallback } from "react";
import { api } from "../api/client";
import { useNavigate } from "react-router-dom";
import Header from "../components/Header";
import Footer from "../components/Footer";
import AnimatedTree from "../components/AnimatedTree";
import dangerousIcon from "../images/dangerous.svg";
import rareIcon from "../images/rare.svg";

// Forest images
import treeBackA from "../images/Nature/forest/tree-backA.svg";
import treeBackB from "../images/Nature/forest/tree-BackB.svg";
import pineForest from "../images/Nature/forest/Pine-forest-back-removebg-preview.png";
import leafA from "../images/Nature/Leaf-A.svg";
import leafB from "../images/Nature/Leaf-B.svg";
import leafC from "../images/Nature/Leaf-C.svg";

// SVG Icons
const GlobeIcon = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="icon-sm">
    <circle cx="12" cy="12" r="10" />
    <line x1="2" y1="12" x2="22" y2="12" />
    <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z" />
  </svg>
);

const MountainIcon = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="icon-sm">
    <path d="m8 3 4 8 5-5 5 15H2L8 3z" />
  </svg>
);

const PaletteIcon = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="icon-sm">
    <circle cx="13.5" cy="6.5" r="0.5" fill="currentColor" />
    <circle cx="17.5" cy="10.5" r="0.5" fill="currentColor" />
    <circle cx="8.5" cy="7.5" r="0.5" fill="currentColor" />
    <circle cx="6.5" cy="12.5" r="0.5" fill="currentColor" />
    <path d="M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10c.926 0 1.648-.746 1.648-1.688 0-.437-.18-.835-.437-1.125-.29-.289-.438-.652-.438-1.125a1.64 1.64 0 0 1 1.668-1.668h1.996c3.051 0 5.555-2.503 5.555-5.555C21.965 6.012 17.461 2 12 2z" />
  </svg>
);

const RulerIcon = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="icon-sm">
    <path d="M21.3 15.3a2.4 2.4 0 0 1 0 3.4l-2.6 2.6a2.4 2.4 0 0 1-3.4 0L2.7 8.7a2.41 2.41 0 0 1 0-3.4l2.6-2.6a2.41 2.41 0 0 1 3.4 0Z" />
    <path d="m14.5 12.5 2-2" />
    <path d="m11.5 9.5 2-2" />
    <path d="m8.5 6.5 2-2" />
    <path d="m17.5 15.5 2-2" />
  </svg>
);

// Dangerous icon (beast jaws)
const JawsIcon = () => (
  <img src={dangerousIcon} alt="Dangerous" className="icon-sm" />
);

// Rare icon (shark jaws)
const BinocularsIcon = () => (
  <img src={rareIcon} alt="Rare" className="icon-sm" />
);

export default function SearchPage() {
  const navigate = useNavigate();

  const [refs, setRefs] = useState({
    countries: [],
    habitats: [],
    colors: [],
    sizes: [],
  });

  const [dangerous, setDangerous] = useState(false);
  const [rare, setRare] = useState(false);

  const [selectedCountries, setSelectedCountries] = useState([]);
  const [selectedHabitats, setSelectedHabitats] = useState([]);
  const [selectedColors, setSelectedColors] = useState([]);
  const [selectedSizes, setSelectedSizes] = useState([]);

  const [dropdowns, setDropdowns] = useState({
    countries: false,
    habitats: false,
    colors: false,
    sizes: false,
  });

  const [isSearching, setIsSearching] = useState(false);

  // Forest animation state
  const [leaves, setLeaves] = useState([]);
  const [isWindy, setIsWindy] = useState(false);
  const [middleTrees, setMiddleTrees] = useState([]);
  const leafImages = [leafA, leafB, leafC];

  // Initialize 5 to 9 random middle trees
  useEffect(() => {
    const treeTypes = [treeBackA, treeBackB];
    const classTypes = ['tree-a', 'tree-b'];
    const numTrees = Math.floor(Math.random() * 5) + 5; // 5 to 9 trees
    const newTrees = [];
    
    // Distribute evenly from left to center (e.g. 0% to 65% width)
    const spacing = 65 / (numTrees - 1);
    
    for (let i = 0; i < numTrees; i++) {
      const typeIndex = Math.floor(Math.random() * 2);
      newTrees.push({
        id: `mid-tree-${i}`,
        src: treeTypes[typeIndex],
        className: classTypes[typeIndex],
        left: `${i * spacing}%`, // Even spacing 0-65%
        bottom: `${10 + Math.random() * 10}%`, // Slightly varied vertical position
        scale: 0.6 + Math.random() * 0.4, // Different sizes
        zIndex: Math.floor(Math.random() * 10) + 10 // Make sure they appear above pine forest
      });
    }
    setMiddleTrees(newTrees);
  }, []);

  // Spawn a falling leaf FROM the tree
  const spawnLeaf = useCallback(() => {
    const id = Date.now() + Math.random();
    const leafType = leafImages[Math.floor(Math.random() * leafImages.length)];
    const baseSize = 18 + Math.random() * 8; // 18-26px (small variance)
    // Start strictly from the right side where the front tree is located
    const startX = 75 + Math.random() * 25; // 75-100% (right side of the screen)
    const startY = 25 + Math.random() * 35; // 25-60% from top (centered on canopy)
    const fallDuration = 8 + Math.random() * 6; // 8-14 seconds (slower, gentle fall)
    const swayAmount = 60 + Math.random() * 80; // More horizontal sway for wave effect

    const newLeaf = {
      id,
      image: leafType,
      size: baseSize,
      startX,
      startY,
      fallDuration,
      swayAmount,
      rotation: Math.random() * 360,
      rotationSpeed: 1.2 + Math.random() * 1.8, // Slower rotation
    };

    setLeaves(prev => [...prev, newLeaf]);

    // Remove after animation
    setTimeout(() => {
      setLeaves(prev => prev.filter(l => l.id !== id));
    }, fallDuration * 1000 + 500);
  }, [leafImages]);

  // Continuous falling leaves effect
  useEffect(() => {
    let isSpawning = true;
    const scheduleNextLeaf = () => {
      if (!isSpawning) return;
      spawnLeaf();
      // Faster delay: 0.5 to 1.5 seconds per leaf for a denser fall
      const delay = 500 + Math.random() * 1000;
      setTimeout(scheduleNextLeaf, delay);
    };
    
    // Quick initial delay so they start falling shortly after load
    const initialDelay = 500 + Math.random() * 1000;
    setTimeout(scheduleNextLeaf, initialDelay);
    
    return () => {
      isSpawning = false;
    };
  }, [spawnLeaf]);

  // Wind effect - occasionally triggers tree sway
  useEffect(() => {
    const triggerWind = () => {
      setIsWindy(true);
      // Wind lasts 3-6 seconds
      const windDuration = 3000 + Math.random() * 3000;
      setTimeout(() => setIsWindy(false), windDuration);
      
      // Spawn just 1 or 2 extra leaves during wind instead of a huge burst
      const leafCount = 1 + Math.floor(Math.random() * 2); 
      for (let i = 0; i < leafCount; i++) {
        setTimeout(() => spawnLeaf(), i * 600);
      }
    };

    // Initial wind after 4 seconds
    const initialTimeout = setTimeout(triggerWind, 4000);

    // Random wind intervals (10-25 seconds)
    const scheduleWind = () => {
      const delay = 10000 + Math.random() * 15000;
      return setTimeout(() => {
        triggerWind();
        windTimerId = scheduleWind();
      }, delay);
    };

    let windTimerId = scheduleWind();

    return () => {
      clearTimeout(initialTimeout);
      clearTimeout(windTimerId);
    };
  }, [spawnLeaf]);

  // Load references
  useEffect(() => {
    Promise.all([
      api.get("/api/reference/countries"),
      api.get("/api/reference/habitats"),
      api.get("/api/reference/colors"),
      api.get("/api/reference/sizes"),
    ]).then(([c, h, co, s]) => {
      setRefs({
        countries: c.data,
        habitats: h.data,
        colors: co.data,
        sizes: s.data,
      });
    });
  }, []);

  // Close dropdowns when clicking outside
  useEffect(() => {
    const handleClickOutside = () => {
      setDropdowns({ countries: false, habitats: false, colors: false, sizes: false });
    };
    document.addEventListener("click", handleClickOutside);
    return () => document.removeEventListener("click", handleClickOutside);
  }, []);

  const toggleDropdown = (field, e) => {
    e.stopPropagation();
    setDropdowns((prev) => ({
      countries: false,
      habitats: false,
      colors: false,
      sizes: false,
      [field]: !prev[field],
    }));
  };

  const selectItem = (selected, setSelected, item) => {
    if (selected.find(s => (s.id || s) === (item.id || item))) return;
    if (selected.length >= 5) return;
    setSelected([...selected, item]);
  };

  const removeItem = (selected, setSelected, item, e) => {
    e.stopPropagation();
    setSelected(selected.filter((i) => (i.id || i) !== (item.id || item)));
  };

  const search = async () => {
    setIsSearching(true);
    try {
      const body = {
        isDangerous: dangerous,
        isRare: rare,
        countryIds: selectedCountries.map((c) => c.id || c),
        habitatIds: selectedHabitats.map((h) => h.id || h),
        colorIds: selectedColors.map((c) => c.id || c),
        sizeIds: selectedSizes.map((s) => s.id || s),
      };

      const res = await api.post("/api/species/search", body);
      navigate("/results", { state: { results: res.data } });
    } catch (err) {
      alert("Search failed: " + (err.response?.data?.message || err.message));
    } finally {
      setIsSearching(false);
    }
  };

  const clearAll = () => {
    setDangerous(false);
    setRare(false);
    setSelectedCountries([]);
    setSelectedHabitats([]);
    setSelectedColors([]);
    setSelectedSizes([]);
  };

  const dropdownFields = [
    { label: "Countries", field: "countries", icon: <GlobeIcon />, items: refs.countries, selected: selectedCountries, setSelected: setSelectedCountries },
    { label: "Habitats", field: "habitats", icon: <MountainIcon />, items: refs.habitats, selected: selectedHabitats, setSelected: setSelectedHabitats },
    { label: "Colors", field: "colors", icon: <PaletteIcon />, items: refs.colors, selected: selectedColors, setSelected: setSelectedColors },
    { label: "Sizes", field: "sizes", icon: <RulerIcon />, items: refs.sizes, selected: selectedSizes, setSelected: setSelectedSizes },
  ];

  const SearchIcon = (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="11" cy="11" r="8" />
      <path d="m21 21-4.35-4.35" />
    </svg>
  );

  const FilterIcon = (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" />
    </svg>
  );

  return (
    <div className="search-page-wrapper">
      {/* Forest Background Animation */}
      <div className={`forest-bg ${isWindy ? 'windy' : ''}`}>
        {/* Pine forest - static repeating background on horizon */}
        <div 
          className="pine-forest-layer"
          style={{
            backgroundImage: `url(${pineForest})`,
            backgroundRepeat: 'repeat-x',
            backgroundPosition: 'bottom center',
            backgroundSize: 'auto 100%',
            position: 'absolute',
            bottom: '15%',
            left: '0',
            width: '100%',
            height: '55%',
            zIndex: 0,
            opacity: 0.4
          }}
        >
        </div>

        {/* Back trees - slight movement in wind, generated randomly */}
        <div className="back-trees-layer">
          {middleTrees.map((tree) => (
            <img 
              key={tree.id} 
              src={tree.src} 
              alt="" 
              className={`back-tree ${tree.className}`} 
              style={{
                position: 'absolute',
                left: tree.left,
                bottom: tree.bottom,
                transform: `scale(${tree.scale})`,
                zIndex: tree.zIndex
              }}
            />
          ))}
        </div>

        {/* Front tree - static on right side */}
        <div className="front-tree-layer">
          <AnimatedTree isWindy={false} />
        </div>

        {/* Falling leaves */}
        <div className="leaves-layer">
          {leaves.map(leaf => (
            <div
              key={leaf.id}
              className="falling-leaf"
              style={{
                '--start-x': `${leaf.startX}%`,
                '--start-y': `${leaf.startY}%`,
                '--fall-duration': `${leaf.fallDuration}s`,
                '--sway-amount': `${leaf.swayAmount}px`,
                '--rotation': `${leaf.rotation}deg`,
                '--rotation-speed': `${leaf.rotationSpeed}s`,
                width: `${leaf.size}px`,
                height: `${leaf.size}px`,
              }}
            >
              <img src={leaf.image} alt="" />
            </div>
          ))}
        </div>

        {/* Atmospheric overlay */}
        <div className="forest-atmosphere"></div>
      </div>

      <Header />

      <main className="search-main">
        <div className="search-container">
          {/* Header Section */}
          <div className="search-header">
            <div className="search-header-icon">
              {FilterIcon}
            </div>
            <h1>Advanced Search</h1>
            <p>Find wildlife species by filtering with multiple criteria</p>
          </div>

          {/* Search Card */}
          <div className="search-card-new">
            {/* Quick Filters */}
            <div className="search-quick-filters">
              <label className={`search-toggle ${dangerous ? 'active' : ''}`}>
                <input 
                  type="checkbox" 
                  checked={dangerous} 
                  onChange={(e) => setDangerous(e.target.checked)} 
                />
                <span className="search-toggle-icon">
                  <JawsIcon />
                </span>
                <span className="search-toggle-text">Dangerous Only</span>
              </label>

              <label className={`search-toggle ${rare ? 'active' : ''}`}>
                <input 
                  type="checkbox" 
                  checked={rare} 
                  onChange={(e) => setRare(e.target.checked)} 
                />
                <span className="search-toggle-icon">
                  <BinocularsIcon />
                </span>
                <span className="search-toggle-text">Rare Species</span>
              </label>
            </div>

            {/* Dropdown Filters */}
            <div className="search-filters-grid">
              {dropdownFields.map(({ label, field, icon, items, selected, setSelected }) => (
                <div key={field} className="search-dropdown">
                  <label className="search-dropdown-label">
                    <span className="search-dropdown-icon">{icon}</span>
                    {label}
                  </label>
                  <div 
                    className={`search-dropdown-input ${dropdowns[field] ? 'open' : ''}`}
                    onClick={(e) => toggleDropdown(field, e)}
                  >
                    <div className="search-dropdown-tags">
                      {selected.length === 0 ? (
                        <span className="search-dropdown-placeholder">Select {label.toLowerCase()}...</span>
                      ) : (
                        selected.map((item) => (
                          <span key={item.id || item} className="search-tag">
                            {item.name || item}
                            <button 
                              className="search-tag-remove"
                              onClick={(e) => removeItem(selected, setSelected, item, e)}
                            >
                              ×
                            </button>
                          </span>
                        ))
                      )}
                    </div>
                    <span className={`search-dropdown-arrow ${dropdowns[field] ? 'rotated' : ''}`}>
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                        <polyline points="6 9 12 15 18 9" />
                      </svg>
                    </span>
                  </div>

                  {dropdowns[field] && (
                    <div className="search-dropdown-menu" onClick={(e) => e.stopPropagation()}>
                      {items.length === 0 ? (
                        <div className="search-dropdown-empty">Loading...</div>
                      ) : (
                        items.map((item) => {
                          const isSelected = selected.find(s => (s.id || s) === (item.id || item));
                          return (
                            <button
                              key={item.id || item}
                              className={`search-dropdown-option ${isSelected ? 'selected' : ''}`}
                              onClick={() => selectItem(selected, setSelected, item)}
                              disabled={isSelected}
                            >
                              {item.name || item}
                              {isSelected && <span className="search-option-check">✓</span>}
                            </button>
                          );
                        })
                      )}
                    </div>
                  )}
                </div>
              ))}
            </div>

            {/* Action Buttons */}
            <div className="search-actions">
              <button className="search-btn-clear" onClick={clearAll}>
                Clear All
              </button>
              <button 
                className="search-btn-submit"
                onClick={search}
                disabled={isSearching}
              >
                {isSearching ? (
                  <>
                    <span className="search-spinner"></span>
                    Searching...
                  </>
                ) : (
                  <>
                    {SearchIcon}
                    Search Species
                  </>
                )}
              </button>
            </div>
          </div>

          {/* Back Link */}
          <button className="search-back-link" onClick={() => navigate("/")}>
            ← Back to Home
          </button>
        </div>
      </main>

      <Footer />
    </div>
  );
}
