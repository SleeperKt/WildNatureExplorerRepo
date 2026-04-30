import { useState, useMemo } from "react";
import { useNavigate, Link } from "react-router-dom";

export default function Header() {
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const isAuthenticated = useMemo(
    () => Boolean(localStorage.getItem("token")),
    []
  );

  const logout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  return (
    <header className="header">
      <div className="header-container">
        <Link to="/" className="logo">
          <svg className="logo-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M12 2L2 7l10 5 10-5-10-5z" />
            <path d="M2 17l10 5 10-5" />
            <path d="M2 12l10 5 10-5" />
          </svg>
          <span className="logo-text">WildNature</span>
        </Link>

        <nav className={`nav ${mobileMenuOpen ? "nav-open" : ""}`}>
          <Link to="/" className="nav-link">Home</Link>
          <Link to="/search" className="nav-link">Explore</Link>
          <Link to="/geo" className="nav-link">Map</Link>
          <Link to="/ai" className="nav-link">AI Assistant</Link>
          {isAuthenticated && (
            <Link to="/library" className="nav-link">Library</Link>
          )}
        </nav>

        <div className="header-actions">
          {isAuthenticated ? (
            <button className="btn btn-outline" onClick={logout}>
              Logout
            </button>
          ) : (
            <>
              <button className="btn btn-outline" onClick={() => navigate("/login")}>
                Sign In
              </button>
              <button className="btn btn-primary" onClick={() => navigate("/login?mode=register")}>
                Sign Up
              </button>
            </>
          )}
        </div>

        <button 
          className="mobile-menu-btn"
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          aria-label="Toggle menu"
        >
          <span className={`hamburger ${mobileMenuOpen ? "open" : ""}`}></span>
        </button>
      </div>
    </header>
  );
}
