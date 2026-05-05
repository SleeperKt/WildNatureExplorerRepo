import { useGlobalError } from '../context/ErrorContext';

export default function GlobalErrorModal() {
  const { backendError, showBackendErrorModal, hideError } = useGlobalError();

  if (!showBackendErrorModal) return null;

  return (
    <div className="error-modal-overlay">
      <div className="error-modal-box">
        <div className="error-modal-icon">
          <svg
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
          >
            <circle cx="12" cy="12" r="10" />
            <path d="M12 8v4M12 16h.01" />
          </svg>
        </div>
        <h2 className="error-modal-title">Connection Error</h2>
        <p className="error-modal-message">{backendError}</p>
        <button className="error-modal-button" onClick={hideError}>
          Try Again
        </button>
      </div>
    </div>
  );
}
