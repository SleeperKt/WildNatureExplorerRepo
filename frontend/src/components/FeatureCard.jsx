import { useNavigate } from 'react-router-dom';

export default function FeatureCard({
  icon,
  title,
  description,
  link,
  requiresAuth = false,
  onBetaClick = null,
  isBeta = false,
}) {
  const navigate = useNavigate();
  const isAuthenticated = Boolean(localStorage.getItem('token'));

  const handleClick = () => {
    if (isBeta && onBetaClick) {
      onBetaClick();
      return;
    }
    if (requiresAuth && !isAuthenticated) {
      navigate(`/login?redirect=${encodeURIComponent(link)}`);
      return;
    }
    navigate(link);
  };

  return (
    <div className="feature-card" onClick={handleClick}>
      <div className="feature-icon">{icon}</div>
      <h3 className="feature-title">{title}</h3>
      <p className="feature-description">{description}</p>
      <span className="feature-link">
        Learn more
        <svg
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
        >
          <path d="M5 12h14M12 5l7 7-7 7" />
        </svg>
      </span>
    </div>
  );
}
