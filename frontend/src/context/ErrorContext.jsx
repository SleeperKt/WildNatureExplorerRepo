import { createContext, useContext, useState } from 'react';

const ErrorContext = createContext();

export const ErrorProvider = ({ children }) => {
  const [backendError, setBackendError] = useState('');
  const [showBackendErrorModal, setShowBackendErrorModal] = useState(false);

  const showError = (message) => {
    setBackendError(message);
    setShowBackendErrorModal(true);
  };

  const hideError = () => {
    setShowBackendErrorModal(false);
    setBackendError('');
  };

  return (
    <ErrorContext.Provider
      value={{ backendError, showBackendErrorModal, showError, hideError }}
    >
      {children}
    </ErrorContext.Provider>
  );
};

// Hook lives alongside provider in small apps; split files if fast-refresh complains during dev.
// eslint-disable-next-line react-refresh/only-export-components -- intentional paired hook export
export const useGlobalError = () => {
  const context = useContext(ErrorContext);
  if (!context) {
    throw new Error('useGlobalError must be used within ErrorProvider');
  }
  return context;
};
