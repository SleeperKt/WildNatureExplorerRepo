import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../api/client';
import Header from '../components/Header';
import Footer from '../components/Footer';
import FeatureCard from '../components/FeatureCard';

// Nature SVG imports
import grassA from '../images/Nature/grass-A.svg';
import grassB from '../images/Nature/grass-B.svg';
import treeA from '../images/Nature/Tree-A.svg';
import treeB from '../images/Nature/Tree-B.svg';
import LeafA from '../images/Nature/Leaf-A.svg';

export default function MainPage() {
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [suggestions, setSuggestions] = useState([]);
  const [isSearchFocused, setIsSearchFocused] = useState(false);
  const [searchResult, setSearchResult] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [showResultCard, setShowResultCard] = useState(false);

  const token = localStorage.getItem('token');

  // Fetch autocomplete suggestions
  useEffect(() => {
    if (!query || query.length < 2) {
      setSuggestions([]);
      return;
    }

    const fetchSuggestions = async () => {
      try {
        const res = await api.get('/api/species/autocomplete', {
          params: { Prefix: query },
          headers: token ? { Authorization: `Bearer ${token}` } : {},
        });
        setSuggestions(res.data);
      } catch (err) {
        console.error(err);
        setSuggestions([]);
      }
    };

    const debounce = setTimeout(fetchSuggestions, 300);
    return () => clearTimeout(debounce);
  }, [query, token]);

  // Search and show result card
  const handleSearch = async (searchTerm) => {
    const term = searchTerm || query;
    if (!term.trim()) return;

    setIsLoading(true);
    setSuggestions([]);
    setIsSearchFocused(false);

    try {
      const res = await api.get('/api/species/by-name', {
        params: { name: term },
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });
      setSearchResult(res.data);
      setShowResultCard(true);
    } catch (err) {
      console.error(err);
      // Fallback to search page if direct lookup fails
      navigate(`/results?q=${encodeURIComponent(term)}`);
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  const closeResultCard = () => {
    setShowResultCard(false);
    setSearchResult(null);
  };

  // Feature card icons
  const SearchIcon = (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <circle cx="11" cy="11" r="8" />
      <path d="m21 21-4.35-4.35" />
    </svg>
  );

  const MapIcon = (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
      <circle cx="12" cy="10" r="3" />
    </svg>
  );

  const AiIcon = (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M12 2a2 2 0 0 1 2 2c0 .74-.4 1.39-1 1.73V7h1a7 7 0 0 1 7 7h1a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-1v1a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-1H2a1 1 0 0 1-1-1v-3a1 1 0 0 1 1-1h1a7 7 0 0 1 7-7h1V5.73c-.6-.34-1-.99-1-1.73a2 2 0 0 1 2-2z" />
      <path d="M9 14v2" />
      <path d="M15 14v2" />
    </svg>
  );

  const LibraryIcon = (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z" />
      <path d="M9 7h6" />
      <path d="M9 11h4" />
    </svg>
  );

  return (
    <div className="page-wrapper">
      <Header />

      {/* Hero Section */}
      <section className="hero">
        <div className="hero-background">
          <div className="hero-gradient"></div>
          <div className="hero-pattern"></div>
        </div>

        <div className="hero-content">
          <span className="hero-badge">Explore Wildlife Worldwide</span>
          <h1 className="hero-title">
            Discover the <span className="highlight">Wild Nature</span>
          </h1>
          <p className="hero-subtitle">
            Your gateway to exploring thousands of species, their habitats, and
            conservation status. Powered by AI for intelligent wildlife
            identification.
          </p>

          {/* Search Bar */}
          <div className="hero-search-wrapper">
            <div className={`hero-search ${isSearchFocused ? 'focused' : ''}`}>
              <div className="search-icon">
                <svg
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <circle cx="11" cy="11" r="8" />
                  <path d="m21 21-4.35-4.35" />
                </svg>
              </div>
              <input
                type="text"
                placeholder="Search for any species..."
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                onFocus={() => setIsSearchFocused(true)}
                onBlur={() => setTimeout(() => setIsSearchFocused(false), 300)}
                onKeyDown={handleKeyDown}
              />
              <button
                className="search-btn"
                onClick={() => handleSearch()}
                disabled={isLoading}
              >
                {isLoading ? '...' : 'Search'}
              </button>
            </div>

            {/* Autocomplete Dropdown - positioned outside search box */}
            {suggestions.length > 0 && isSearchFocused && (
              <ul className="search-suggestions">
                {suggestions.slice(0, 5).map((s, idx) => (
                  <li
                    key={idx}
                    onMouseDown={(e) => {
                      e.preventDefault();
                      setQuery(s);
                      handleSearch(s);
                    }}
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
                    <span>{s}</span>
                  </li>
                ))}
              </ul>
            )}
          </div>

          {/* Quick Stats */}
          <div className="hero-stats">
            <div className="stat">
              <span className="stat-number">10K+</span>
              <span className="stat-label">Species</span>
            </div>
            <div className="stat-divider"></div>
            <div className="stat">
              <span className="stat-number">150+</span>
              <span className="stat-label">Countries</span>
            </div>
            <div className="stat-divider"></div>
            <div className="stat">
              <span className="stat-number">AI</span>
              <span className="stat-label">Powered</span>
            </div>
          </div>

          {/* Scroll Indicator - inside hero-content, below stats */}
          <div className="scroll-indicator">
            <span>Scroll to explore</span>
            <div className="scroll-arrow">
              <svg
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <path d="M12 5v14M19 12l-7 7-7-7" />
              </svg>
            </div>
          </div>
        </div>

        {/* Decorative Elements - Nature Scene */}
        <div className="hero-decoration">
          {/* Mountain Landscape - Two peaks in the background */}
          <div className="mountain-landscape">
            <svg
              className="mountain mountain-left"
              viewBox="0 0 400 300"
              preserveAspectRatio="none"
            >
              <path
                d="M0 300 L0 300 L200 50 L400 300 Z"
                fill="rgba(255,255,255,0.06)"
              />
            </svg>
            <svg
              className="mountain mountain-right"
              viewBox="0 0 400 300"
              preserveAspectRatio="none"
            >
              <path
                d="M0 300 L200 30 L400 300 Z"
                fill="rgba(255,255,255,0.05)"
              />
            </svg>
          </div>

          {/* Floating Leaves */}
          <div className="floating-leaf leaf-1"></div>
          <div className="floating-leaf leaf-2"></div>
          <div className="floating-leaf leaf-3"></div>
          <div className="floating-leaf leaf-4"></div>
          <div className="floating-leaf leaf-5"></div>

          {/* Flying Birds */}
          <div className="flying-bird bird-1"></div>
          <div className="flying-bird bird-2"></div>
          <div className="flying-bird bird-3"></div>

          {/* Tree Silhouettes - using SVG images */}
          <svg
            className="tree-svg tree-left"
            viewBox="0 0 100 200"
            fill="rgba(255,255,255,0.08)"
          >
            <path d="M50 0 L30 50 L40 50 L20 100 L35 100 L10 150 L40 150 L40 200 L60 200 L60 150 L90 150 L65 100 L80 100 L60 50 L70 50 Z" />
          </svg>
          <svg
            className="tree-svg tree-center"
            viewBox="0 0 80 180"
            fill="rgba(255,255,255,0.04)"
          >
            <path d="M40 0 L20 60 L30 60 L10 120 L25 120 L0 180 L38 180 L38 180 L42 180 L42 180 L80 180 L55 120 L70 120 L50 60 L60 60 Z" />
          </svg>

          {/* Additional Tree Images - 8 trees (left and center only) */}
          <img src={treeA} alt="" className="tree-img tree-img-1" />
          <img src={treeA} alt="" className="tree-img tree-img-2" />
          <img src={treeB} alt="" className="tree-img tree-img-3" />
          <img src={treeB} alt="" className="tree-img tree-img-4" />
          <img src={treeA} alt="" className="tree-img tree-img-5" />
          <img src={treeB} alt="" className="tree-img tree-img-6" />
          <img src={treeA} alt="" className="tree-img tree-img-7" />
          <img src={treeB} alt="" className="tree-img tree-img-8" />

          {/* Animal Silhouettes - using proper SVGs */}
          <svg
            className="animal-svg deer-svg"
            viewBox="0 0 100 80"
            fill="rgba(255,255,255,0.06)"
          >
            <path d="M15 20 L15 5 L18 10 L20 5 L20 20 M20 25 Q25 15 35 20 Q45 25 45 40 L45 60 L40 75 L42 75 L45 60 L55 60 L55 75 L58 75 L55 55 Q60 50 65 55 L65 75 L68 75 L68 55 Q75 50 80 55 L80 75 L83 75 L80 50 Q85 45 85 35 Q85 25 75 20 Q65 15 55 20 Q50 22 45 25 L35 25 Q25 20 20 25 Z" />
          </svg>
          <svg
            className="animal-svg elephant-svg"
            viewBox="0 0 120 80"
            fill="rgba(255,255,255,0.05)"
          >
            <path d="M20 40 Q10 35 5 45 Q0 55 10 60 L10 75 L15 75 L15 60 Q20 55 25 60 L25 75 L30 75 L30 55 L70 55 L70 75 L75 75 L75 55 L85 55 L85 75 L90 75 L90 55 Q100 50 105 40 Q110 30 100 25 Q90 20 80 25 Q75 15 60 15 Q45 15 40 25 Q35 20 25 25 Q15 30 20 40 Z M30 35 A3 3 0 1 0 30 35.1" />
          </svg>
          <svg
            className="animal-svg lion-svg"
            viewBox="0 0 100 70"
            fill="rgba(255,255,255,0.05)"
          >
            <ellipse cx="50" cy="20" rx="25" ry="18" />
            <circle cx="50" cy="25" r="12" />
            <path d="M35 35 Q30 45 35 55 L35 65 L40 65 L40 55 L60 55 L60 65 L65 65 L65 55 Q70 45 65 35 Q60 40 50 40 Q40 40 35 35 Z" />
            <circle cx="45" cy="23" r="2" />
            <circle cx="55" cy="23" r="2" />
            <ellipse cx="50" cy="30" rx="3" ry="2" />
          </svg>

          {/* Grass/Plants - dense carpet with grassA and grassB */}
          {Array.from({ length: 50 }, (_, i) => (
            <img
              key={i}
              src={i % 2 === 0 ? grassA : grassB}
              alt=""
              className={`grass-img grass-img-${i + 1}`}
            />
          ))}

          {/* Fireflies/Particles */}
          <div className="firefly firefly-1"></div>
          <div className="firefly firefly-2"></div>
          <div className="firefly firefly-3"></div>
          <div className="firefly firefly-4"></div>
          <div className="firefly firefly-5"></div>
        </div>
      </section>

      {/* Search Result Card - Floating Modal */}
      {showResultCard && searchResult && (
        <div className="result-card-overlay" onClick={closeResultCard}>
          <div
            className="result-card-modal"
            onClick={(e) => e.stopPropagation()}
          >
            <button className="result-card-close" onClick={closeResultCard}>
              <svg
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <path d="M18 6L6 18M6 6l12 12" />
              </svg>
            </button>

            <div className="result-card-header">
              <div className="result-card-icon">
                <svg
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <circle cx="12" cy="12" r="10" />
                  <path d="M12 16v-4M12 8h.01" />
                </svg>
              </div>
              <div className="result-card-titles">
                <h2>{searchResult.commonName}</h2>
                <p className="scientific-name">{searchResult.scientificName}</p>
              </div>
            </div>

            <div className="result-card-body">
              <p className="result-description">{searchResult.description}</p>

              <div className="result-tags">
                {searchResult.isDangerous && (
                  <span className="tag tag-danger">Dangerous</span>
                )}
                {searchResult.isRare && (
                  <span className="tag tag-rare">Rare Species</span>
                )}
              </div>

              <div className="result-details">
                <div className="detail-row">
                  <span className="detail-label">Size</span>
                  <span className="detail-value">{searchResult.size}</span>
                </div>
                {searchResult.colors?.length > 0 && (
                  <div className="detail-row">
                    <span className="detail-label">Colors</span>
                    <span className="detail-value">
                      {searchResult.colors.join(', ')}
                    </span>
                  </div>
                )}
                {searchResult.habitats?.length > 0 && (
                  <div className="detail-row">
                    <span className="detail-label">Habitats</span>
                    <span className="detail-value">
                      {searchResult.habitats.join(', ')}
                    </span>
                  </div>
                )}
                {searchResult.countries?.length > 0 && (
                  <div className="detail-row">
                    <span className="detail-label">Found in</span>
                    <span className="detail-value">
                      {searchResult.countries.slice(0, 5).join(', ')}
                      {searchResult.countries.length > 5
                        ? ` +${searchResult.countries.length - 5} more`
                        : ''}
                    </span>
                  </div>
                )}
              </div>
            </div>

            <div className="result-card-actions">
              <button className="btn btn-outline" onClick={closeResultCard}>
                Close
              </button>
              <button
                className="btn btn-primary"
                onClick={() => navigate('/geo')}
              >
                View on Map
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Features Section */}
      <section className="features">
        <div className="features-header">
          <span className="section-badge">Features</span>
          <h2 className="section-title">Everything You Need to Explore</h2>
          <p className="section-subtitle">
            Powerful tools to discover, learn, and protect wildlife
          </p>
        </div>

        <div className="features-grid">
          <FeatureCard
            icon={SearchIcon}
            title="Advanced Search"
            description="Filter species by habitat, size, color, danger level, and more. Find exactly what you're looking for."
            link="/search"
          />
          <FeatureCard
            icon={MapIcon}
            title="Interactive Map"
            description="Explore wildlife locations around the globe with our interactive geographic visualization."
            link="/geo"
          />
          <FeatureCard
            icon={AiIcon}
            title="AI Assistant"
            description="Upload images for instant species identification or ask questions about any animal."
            link="/ai"
            requiresAuth={true}
          />
          <FeatureCard
            icon={LibraryIcon}
            title="My Library"
            description="Save the animals you've recognised with their location and notes, then revisit them on your private map."
            link="/library"
            requiresAuth={true}
          />
        </div>
      </section>

      {/* CTA Section */}
      <section className="cta">
        <div className="cta-content">
          <h2>Ready to Explore?</h2>
          <p>
            Join thousands of wildlife enthusiasts and start your journey today.
          </p>
          <div className="cta-buttons">
            <button
              className="btn btn-primary btn-lg"
              onClick={() => navigate('/search')}
            >
              Start Exploring
            </button>
            <button
              className="btn btn-outline btn-lg"
              onClick={() => navigate('/ai')}
            >
              Try AI Assistant
            </button>
          </div>
        </div>
      </section>

      <Footer />
    </div>
  );
}
