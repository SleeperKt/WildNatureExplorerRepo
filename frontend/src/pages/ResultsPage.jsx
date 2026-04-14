import { useState, useEffect, useCallback } from "react";
import { api } from "../api/client";
import { useLocation, useNavigate } from "react-router-dom";
import Header from "../components/Header";
import Footer from "../components/Footer";
import AnimatedTree from "../components/AnimatedTree";
import dangerousIcon from "../images/dangerous.svg";
import rareIcon from "../images/rare.svg";

// Forest images
import treeBackA from "../images/Nature/forest/tree-backA.svg";
import treeBackB from "../images/Nature/forest/tree-BackB.svg";
import treeBackC from "../images/Nature/forest/tree-BackC.svg";
import pineForest from "../images/Nature/forest/Pine-forest-back-removebg-preview.png";
import leafA from "../images/Nature/Leaf-A.svg";
import leafB from "../images/Nature/Leaf-B.svg";
import leafC from "../images/Nature/Leaf-C.svg";

// SVG Icons
const SearchResultIcon = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="icon-lg">
    <circle cx="11" cy="11" r="8" />
    <path d="m21 21-4.35-4.35" />
  </svg>
);

const LizardIcon = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="icon-xl">
    <path d="M13 4v1a3 3 0 0 0 3 3h1" />
    <path d="M18 8h-1a3 3 0 0 1-3-3V4" />
    <path d="M16 21h-4a2 2 0 0 1-2-2v-4a3 3 0 0 1 3-3h7" />
    <path d="M22 12v2a1 1 0 0 1-1 1h-1" />
    <path d="M5 11c1.5 0 3-1.5 3-3 0-1-.5-1.5-1-2" />
    <path d="M2 16c2 0 3-1 4-2" />
  </svg>
);

const AnimalIcon = ({ isDangerous, isRare }) => (
  isDangerous ? (
    <img src={dangerousIcon} alt="Dangerous" className="species-icon" />
  ) : isRare ? (
    <img src={rareIcon} alt="Rare" className="species-icon" />
  ) : (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" className="species-icon">
      <path d="M11 20A7 7 0 0 1 9.8 6.1C15.5 5 17 4.48 19 2c1 2 2 4.18 2 8 0 5.5-4.78 10-10 10Z" />
      <path d="M2 21c0-3 1.85-5.36 5.08-6C9.5 14.52 12 13 13 12" />
    </svg>
  )
);

// Dangerous icon (beast jaws)
const JawsIcon = () => (
  <img src={dangerousIcon} alt="Dangerous" className="badge-icon" />
);

// Rare icon (shark jaws)
const BinocularsIcon = () => (
  <img src={rareIcon} alt="Rare" className="badge-icon" />
);

const LeafIcon = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="badge-icon">
    <path d="M11 20A7 7 0 0 1 9.8 6.1C15.5 5 17 4.48 19 2c1 2 2 4.18 2 8 0 5.5-4.78 10-10 10Z" />
    <path d="M2 21c0-3 1.85-5.36 5.08-6C9.5 14.52 12 13 13 12" />
  </svg>
);

const RulerIcon = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="info-icon">
    <path d="M21.3 15.3a2.4 2.4 0 0 1 0 3.4l-2.6 2.6a2.4 2.4 0 0 1-3.4 0L2.7 8.7a2.41 2.41 0 0 1 0-3.4l2.6-2.6a2.41 2.41 0 0 1 3.4 0Z" />
    <path d="m14.5 12.5 2-2" />
    <path d="m11.5 9.5 2-2" />
    <path d="m8.5 6.5 2-2" />
    <path d="m17.5 15.5 2-2" />
  </svg>
);

const PaletteIcon = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="info-icon">
    <circle cx="13.5" cy="6.5" r="0.5" fill="currentColor" />
    <circle cx="17.5" cy="10.5" r="0.5" fill="currentColor" />
    <circle cx="8.5" cy="7.5" r="0.5" fill="currentColor" />
    <circle cx="6.5" cy="12.5" r="0.5" fill="currentColor" />
    <path d="M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10c.926 0 1.648-.746 1.648-1.688 0-.437-.18-.835-.437-1.125-.29-.289-.438-.652-.438-1.125a1.64 1.64 0 0 1 1.668-1.668h1.996c3.051 0 5.555-2.503 5.555-5.555C21.965 6.012 17.461 2 12 2z" />
  </svg>
);

const MountainIcon = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="info-icon">
    <path d="m8 3 4 8 5-5 5 15H2L8 3z" />
  </svg>
);

const GlobeIcon = () => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="info-icon">
    <circle cx="12" cy="12" r="10" />
    <line x1="2" y1="12" x2="22" y2="12" />
    <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z" />
  </svg>
);

export default function ResultsPage() {
  const navigate = useNavigate();
  const [selected, setSelected] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const location = useLocation();
  const results = location.state?.results || [];

  // Forest animation state
  const [leaves, setLeaves] = useState([]);
  const [isWindy, setIsWindy] = useState(false);
  const leafImages = [leafA, leafB, leafC];

  // Spawn a falling leaf FROM the tree
  const spawnLeaf = useCallback(() => {
    const id = Date.now() + Math.random();
    const leafType = leafImages[Math.floor(Math.random() * leafImages.length)];
    const baseSize = 18 + Math.random() * 8; // 18-26px (small variance)
    // Start from tree position (right side, upper portion of tree)
    const startX = 75 + Math.random() * 20; // 75-95% (where tree branches are)
    const startY = 10 + Math.random() * 30; // 10-40% from top (tree canopy area)
    const fallDuration = 5 + Math.random() * 4; // 5-9 seconds (realistic fall)
    const swayAmount = 40 + Math.random() * 60; // How much it sways

    const newLeaf = {
      id,
      image: leafType,
      size: baseSize,
      startX,
      startY,
      fallDuration,
      swayAmount,
      rotation: Math.random() * 360,
      rotationSpeed: 0.8 + Math.random() * 1.2,
    };

    setLeaves(prev => [...prev, newLeaf]);

    // Remove after animation
    setTimeout(() => {
      setLeaves(prev => prev.filter(l => l.id !== id));
    }, fallDuration * 1000 + 500);
  }, [leafImages]);

  // Wind effect - triggers leaves and tree sway
  useEffect(() => {
    const triggerWind = () => {
      setIsWindy(true);
      // Spawn burst of leaves during wind
      const leafCount = 4 + Math.floor(Math.random() * 4); // 4-7 leaves
      for (let i = 0; i < leafCount; i++) {
        setTimeout(() => spawnLeaf(), i * 300);
      }
      // Wind lasts 3-5 seconds
      const windDuration = 3000 + Math.random() * 2000;
      setTimeout(() => setIsWindy(false), windDuration);
    };

    // Initial wind after 2 seconds
    const initialTimeout = setTimeout(triggerWind, 2000);

    // Random wind intervals (8-20 seconds)
    const scheduleWind = () => {
      const delay = 8000 + Math.random() * 12000;
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

  const openDetails = async (id) => {
    setIsLoading(true);
    try {
      const res = await api.get(`/api/species/${id}`);
      setSelected(res.data);
    } catch (err) {
      alert("Failed to load details: " + (err.response?.data?.message || err.message));
    } finally {
      setIsLoading(false);
    }
  };

  const CloseIcon = (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <line x1="18" y1="6" x2="6" y2="18" />
      <line x1="6" y1="6" x2="18" y2="18" />
    </svg>
  );

  return (
    <div className="results-page-wrapper">
      {/* Forest Background Animation */}
      <div className={`forest-bg ${isWindy ? 'windy' : ''}`}>
        {/* Pine forest - static background with atmospheric effect */}
        <div className="pine-forest-layer">
          <img src={pineForest} alt="" />
        </div>

        {/* Back trees - slight movement in wind */}
        <div className="back-trees-layer">
          <img src={treeBackA} alt="" className="back-tree tree-a" />
          <img src={treeBackB} alt="" className="back-tree tree-b" />
          <img src={treeBackC} alt="" className="back-tree tree-c" />
        </div>

        {/* Front tree - half visible on right side with GSAP animation */}
        <div className="front-tree-layer">
          <AnimatedTree isWindy={isWindy} />
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

      <main className="results-main">
        <div className="results-container">
          {/* Header Section */}
          <div className="results-header">
            <div className="results-header-icon">
              <SearchResultIcon />
            </div>
            <h1>Search Results</h1>
            <p>{results.length} species found matching your criteria</p>
          </div>

          {/* Results Grid */}
          {results.length === 0 ? (
            <div className="results-empty">
              <div className="results-empty-icon">
                <LizardIcon />
              </div>
              <h3>No Species Found</h3>
              <p>Try adjusting your search filters to find more wildlife</p>
              <button className="results-btn-search" onClick={() => navigate("/search")}>
                Modify Search
              </button>
            </div>
          ) : (
            <div className="results-grid-new">
              {results.map((item) => (
                <div
                  key={item.id}
                  className="results-species-card"
                  onClick={() => openDetails(item.id)}
                >
                  <div className={`results-card-image ${item.isDangerous ? 'danger' : item.isRare ? 'rare' : 'common'}`}>
                    <AnimalIcon isDangerous={item.isDangerous} isRare={item.isRare} />
                  </div>
                  <div className="results-card-content">
                    <h3 className="results-card-name">{item.commonName}</h3>
                    <div className="results-card-badges">
                      {item.isRare && (
                        <span className="results-badge rare">
                          <BinocularsIcon /> Rare
                        </span>
                      )}
                      {item.isDangerous && (
                        <span className="results-badge danger">
                          <JawsIcon /> Dangerous
                        </span>
                      )}
                      {!item.isRare && !item.isDangerous && (
                        <span className="results-badge common">
                          <LeafIcon /> Common
                        </span>
                      )}
                    </div>
                  </div>
                  <div className="results-card-arrow">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <polyline points="9 18 15 12 9 6" />
                    </svg>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Back Button */}
          <div className="results-actions">
            <button className="results-back-link" onClick={() => navigate("/search")}>
              ← Back to Search
            </button>
            <button className="results-home-link" onClick={() => navigate("/")}>
              Home
            </button>
          </div>
        </div>
      </main>

      <Footer />

      {/* Species Detail Modal */}
      {selected && (
        <div className="results-modal-overlay" onClick={() => setSelected(null)}>
          <div className="results-modal" onClick={(e) => e.stopPropagation()}>
            <button className="results-modal-close" onClick={() => setSelected(null)}>
              {CloseIcon}
            </button>

            <div className="results-modal-header">
              <div className={`results-modal-icon ${selected.isDangerous ? 'danger' : selected.isRare ? 'rare' : 'common'}`}>
                <AnimalIcon isDangerous={selected.isDangerous} isRare={selected.isRare} />
              </div>
              <div className="results-modal-titles">
                <h2>{selected.commonName}</h2>
                <p className="results-modal-scientific">{selected.scientificName}</p>
              </div>
            </div>

            <div className="results-modal-badges">
              {selected.isRare && <span className="results-badge rare"><BinocularsIcon /> Rare</span>}
              {selected.isDangerous && <span className="results-badge danger"><JawsIcon /> Dangerous</span>}
              {!selected.isRare && !selected.isDangerous && <span className="results-badge common"><LeafIcon /> Common</span>}
            </div>

            <div className="results-modal-body">
              <div className="results-modal-section">
                <h4>Description</h4>
                <p>{selected.description}</p>
              </div>

              <div className="results-modal-info">
                <div className="results-modal-info-item">
                  <span className="results-info-icon">
                    <RulerIcon />
                  </span>
                  <div>
                    <strong>Size</strong>
                    <p>{selected.size}</p>
                  </div>
                </div>

                <div className="results-modal-info-item">
                  <span className="results-info-icon">
                    <PaletteIcon />
                  </span>
                  <div>
                    <strong>Colors</strong>
                    <p>{selected.colors?.join(", ") || "N/A"}</p>
                  </div>
                </div>

                <div className="results-modal-info-item">
                  <span className="results-info-icon">
                    <MountainIcon />
                  </span>
                  <div>
                    <strong>Habitats</strong>
                    <p>{selected.habitats?.join(", ") || "N/A"}</p>
                  </div>
                </div>

                <div className="results-modal-info-item">
                  <span className="results-info-icon">
                    <GlobeIcon />
                  </span>
                  <div>
                    <strong>Countries</strong>
                    <p>{selected.countries?.join(", ") || "N/A"}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Loading Overlay */}
      {isLoading && (
        <div className="results-loading-overlay">
          <div className="results-loading-spinner"></div>
        </div>
      )}
    </div>
  );
}
