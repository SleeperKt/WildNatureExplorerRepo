import { Link } from "react-router-dom";
import grassA from "../images/Nature/grass-A.svg";
import grassB from "../images/Nature/grass-B.svg";

export default function Footer() {
  return (
    <footer className="footer">
      {/* Nature decoration container */}
      <div className="footer-nature-decoration">
        {/* Grass */}
        {Array.from({ length: 50 }, (_, i) => (
          <img
            key={i}
            src={i % 2 === 0 ? grassA : grassB}
            alt=""
            className={`footer-grass footer-grass-${i + 1}`}
          />
        ))}
      </div>

      <div className="footer-container">
        <div className="footer-brand">
          <Link to="/" className="footer-logo">
            <svg className="logo-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M12 2L2 7l10 5 10-5-10-5z" />
              <path d="M2 17l10 5 10-5" />
              <path d="M2 12l10 5 10-5" />
            </svg>
            <span>WildNature Explorer</span>
          </Link>
          <p className="footer-tagline">Discover the wonders of wildlife around the world</p>
        </div>

        <div className="footer-links">
          <div className="footer-column">
            <h4>Explore</h4>
            <Link to="/search">Search Species</Link>
            <Link to="/geo">Wildlife Map</Link>
            <Link to="/ai">AI Assistant</Link>
          </div>
          <div className="footer-column">
            <h4>Resources</h4>
            <a href="#">Documentation</a>
            <a href="#">API Reference</a>
            <a href="#">Conservation</a>
          </div>
          <div className="footer-column">
            <h4>Connect</h4>
            <a href="https://github.com/SleeperKt/WildNatureExplorerRepo" target="_blank" rel="noopener noreferrer">GitHub</a>
            <a href="https://x.com/NatureWiil50398" target="_blank" rel="noopener noreferrer">X (Twitter)</a>
            <a href="mailto:wiildnatureexplorer@gmail.com">Contact</a>
          </div>
        </div>
      </div>

      <div className="footer-bottom">
        <p>&copy; 2026 WildNature Explorer. All rights reserved.</p>
      </div>
    </footer>
  );
}
