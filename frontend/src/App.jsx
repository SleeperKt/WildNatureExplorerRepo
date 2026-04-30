import { BrowserRouter, Routes, Route, useLocation } from "react-router-dom";
import { useEffect } from "react";
import { ErrorProvider } from "./context/ErrorContext";
import GlobalErrorModal from "./components/GlobalErrorModal";
import LoginPage from "./pages/LoginPage";
import MainPage from "./pages/MainPage";
import SearchPage from "./pages/SearchPage";
import GeoPage from "./pages/GeoPage";
import AiPage from "./pages/AiPage";
import LibraryPage from "./pages/LibraryPage";
import ResultsPage from "./pages/ResultsPage";
import { setGlobalErrorHandler } from "./api/client";
import { useGlobalError } from "./context/ErrorContext";

function ScrollToTop() {
  const { pathname } = useLocation();
  
  useEffect(() => {
    window.scrollTo(0, 0);
  }, [pathname]);
  
  return null;
}

function AppContent() {
  const { showError } = useGlobalError();

  useEffect(() => {
    setGlobalErrorHandler(showError);
  }, [showError]);

  return (
    <>
      <GlobalErrorModal />
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/" element={<MainPage />} />
        <Route path="/search" element={<SearchPage />} />
        <Route path="/results" element={<ResultsPage />} />
        <Route path="/geo" element={<GeoPage />} />
        <Route path="/ai" element={<AiPage />} />
        <Route path="/library" element={<LibraryPage />} />
      </Routes>
    </>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <ErrorProvider>
        <ScrollToTop />
        <AppContent />
      </ErrorProvider>
    </BrowserRouter>
  );
}