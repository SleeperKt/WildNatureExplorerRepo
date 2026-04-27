import { useState, useRef, useEffect, useCallback } from "react";
import { api } from "../api/client";
import { useNavigate } from "react-router-dom";
import Header from "../components/Header";
import Footer from "../components/Footer";

// Eagle SVG imports - A, B, C frames for wing animation
import eagleA from "../images/Animal/Eagle/EagleA.svg";
import eagleB from "../images/Animal/Eagle/EagleB.svg";
import eagleC from "../images/Animal/Eagle/EagleC.svg";

// Cloud images - 7 types for variety
import cloudA from "../images/Nature/clouds/CloudA-removebg-preview.png";
import cloudB from "../images/Nature/clouds/CloudB-removebg-preview.png";
import cloudC from "../images/Nature/clouds/CloudC-removebg-preview.png";
import cloudD from "../images/Nature/clouds/CloudD-removebg-preview.png";
import cloudE from "../images/Nature/clouds/CloudE-removebg-preview.png";
import cloudF from "../images/Nature/clouds/CloudF-removebg-preview.png";
import cloudG from "../images/Nature/clouds/CloudG-removebg-preview.png";

const CLOUD_IMAGES = [cloudA, cloudB, cloudC, cloudD, cloudE, cloudF, cloudG];

// Bird size presets (realistic scaling)
const BIRD_SIZES = [
  { width: 120, height: 75 },   // Small distant bird
  { width: 150, height: 95 },   // Medium-small
  { width: 180, height: 115 },  // Medium
  { width: 210, height: 130 },  // Medium-large
  { width: 240, height: 150 },  // Large close bird
];

// Cloud size presets - wider range for realism
const CLOUD_SIZES = [
  { width: 150, height: 90 },   // Tiny wisp
  { width: 220, height: 130 },  // Small
  { width: 320, height: 190 },  // Medium-small
  { width: 450, height: 270 },  // Medium
  { width: 580, height: 350 },  // Large
  { width: 750, height: 450 },  // Very large
  { width: 900, height: 540 },  // Massive
];

export default function AiPage() {
  const navigate = useNavigate();
  const [file, setFile] = useState(null);
  const [sessionId, setSessionId] = useState(null);
  const [currentAnimal, setCurrentAnimal] = useState(null);
  const [messages, setMessages] = useState([]);
  const [question, setQuestion] = useState("");
  const [feedbackOpen, setFeedbackOpen] = useState(false);
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");
  const [isDragOver, setIsDragOver] = useState(false);
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [isAsking, setIsAsking] = useState(false);
  const [recognizer, setRecognizer] = useState("huggingface");
  const [birds, setBirds] = useState([]);
  const [clouds, setClouds] = useState([]);

  const chatRef = useRef(null);
  const fileInputRef = useRef(null);
  const createdUrlsRef = useRef([]);
  const birdIdRef = useRef(0);
  const cloudIdRef = useRef(0);

  // Spawn a random bird
  const spawnBird = useCallback(() => {
    const id = birdIdRef.current++;
    const size = BIRD_SIZES[Math.floor(Math.random() * BIRD_SIZES.length)];
    const fromLeft = Math.random() > 0.5;
    const topPosition = 5 + Math.random() * 30; // 5% to 35% from top
    const flightDuration = 12 + Math.random() * 10; // 12-22 seconds (realistic)
    const flapSpeed = 0.4 + Math.random() * 0.3; // 0.4-0.7s flap cycle
    const wobble = -10 - Math.random() * 30; // vertical wobble -10 to -40px

    const newBird = {
      id,
      size,
      fromLeft,
      topPosition,
      flightDuration,
      flapSpeed,
      wobble,
      createdAt: Date.now(),
    };

    setBirds(prev => [...prev, newBird]);

    // Remove bird after animation completes
    setTimeout(() => {
      setBirds(prev => prev.filter(b => b.id !== id));
    }, flightDuration * 1000 + 500);
  }, []);

  // Random bird spawning system
  useEffect(() => {
    // Spawn initial birds
    const initialCount = 2 + Math.floor(Math.random() * 3); // 2-4 birds initially
    for (let i = 0; i < initialCount; i++) {
      setTimeout(() => spawnBird(), i * 800);
    }

    // Spawn birds at random intervals (3-8 seconds)
    const spawnInterval = () => {
      const delay = 3000 + Math.random() * 5000;
      return setTimeout(() => {
        spawnBird();
        timerId = spawnInterval();
      }, delay);
    };

    let timerId = spawnInterval();

    return () => clearTimeout(timerId);
  }, [spawnBird]);

  // Spawn a random cloud
  const spawnCloud = useCallback(() => {
    const id = cloudIdRef.current++;
    const size = CLOUD_SIZES[Math.floor(Math.random() * CLOUD_SIZES.length)];
    const image = CLOUD_IMAGES[Math.floor(Math.random() * CLOUD_IMAGES.length)];
    const fromLeft = Math.random() > 0.35; // 65% from left (prevailing wind)
    const topPosition = -5 + Math.random() * 50; // -5% to 45% from top
    const driftDuration = 45 + Math.random() * 100; // 45-145 seconds
    const opacity = 0.4 + Math.random() * 0.5; // 0.4 to 0.9 opacity
    const scale = 0.7 + Math.random() * 0.6; // 0.7 to 1.3 scale variation
    const zIndex = Math.floor(Math.random() * 3); // Layer depth 0-2

    const newCloud = {
      id,
      size,
      image,
      fromLeft,
      topPosition,
      driftDuration,
      opacity,
      scale,
      zIndex,
      createdAt: Date.now(),
    };

    setClouds(prev => [...prev, newCloud]);

    // Remove cloud after animation completes
    setTimeout(() => {
      setClouds(prev => prev.filter(c => c.id !== id));
    }, driftDuration * 1000 + 500);
  }, []);

  // Cloud spawning system
  useEffect(() => {
    // Spawn initial clouds - more for fuller sky
    const initialCount = 8 + Math.floor(Math.random() * 5); // 8-12 clouds initially
    for (let i = 0; i < initialCount; i++) {
      setTimeout(() => spawnCloud(), i * 600);
    }

    // Spawn clouds at random intervals (3-8 seconds)
    const spawnInterval = () => {
      const delay = 3000 + Math.random() * 5000;
      return setTimeout(() => {
        spawnCloud();
        cloudTimerId = spawnInterval();
      }, delay);
    };

    let cloudTimerId = spawnInterval();

    return () => clearTimeout(cloudTimerId);
  }, [spawnCloud]);

  // Start session on mount
  useEffect(() => {
    let mounted = true;
    (async () => {
      try {
        const res = await api.post(`/api/ai/start`);
        if (mounted) setSessionId(res.data.sessionId);
      } catch (err) {
        console.warn("Failed to start AI session on mount:", err);
      }
    })();
    return () => { mounted = false; };
  }, []);

  useEffect(() => {
    if (chatRef.current) chatRef.current.scrollTop = chatRef.current.scrollHeight;
  }, [messages]);

  useEffect(() => {
    return () => {
      if (sessionId) {
        void api.post(`/api/ai/end/${sessionId}`).catch((err) => console.error("Failed to end session:", err));
      }
      if (createdUrlsRef.current.length) {
        createdUrlsRef.current.forEach((u) => {
          try { URL.revokeObjectURL(u); } catch { }
        });
        createdUrlsRef.current = [];
      }
    };
  }, [sessionId]);

  const handleDragOver = (e) => {
    e.preventDefault();
    setIsDragOver(true);
  };

  const handleDragLeave = (e) => {
    e.preventDefault();
    setIsDragOver(false);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    setIsDragOver(false);
    const droppedFile = e.dataTransfer.files[0];
    if (droppedFile && droppedFile.type.startsWith('image/')) {
      setFile(droppedFile);
    }
  };

  const analyze = async () => {
    if (!file) return;
    if (!sessionId) return alert("Session not ready. Please wait or refresh the page.");

    setIsAnalyzing(true);
    const form = new FormData();
    form.append("image", file);

    try {
      const res = await api.post(`/api/ai/analyze/${sessionId}?recognizer=${encodeURIComponent(recognizer)}`, form);
      const newSessionId = res.data.sessionId;
      if (!sessionId) setSessionId(newSessionId);
      
      const animal = res.data.animal;
      setCurrentAnimal(animal);
      const fileUrl = URL.createObjectURL(file);
      createdUrlsRef.current.push(fileUrl);
      setMessages((prev) => [
        ...prev,
        {
          type: "bot",
          content: (
            <>
              <p className="ai-animal-name">{animal.name}</p>
              <p className="ai-animal-desc">{animal.description}</p>
              <div className="ai-animal-details">
                <span><strong>Habitat:</strong> {animal.habitat}</span>
                <span><strong>Danger:</strong> {animal.dangerLevel}</span>
                <span><strong>Rarity:</strong> {animal.rarityLevel}</span>
              </div>
            </>
          ),
          fileUrl,
          isImage: true,
        },
      ]);
    } catch (err) {
      alert("Analysis failed: " + (err.response?.data?.message || err.message));
    } finally {
      setIsAnalyzing(false);
    }
  };

  const ask = async () => {
    if (!question.trim() || isAsking) return;

    setMessages((prev) => [...prev, { type: "user", content: question }]);
    setIsAsking(true);
    
    try {
      let finalQuestion = question;
      if (currentAnimal) {
        finalQuestion = `We are discussing a ${currentAnimal.name}. ${currentAnimal.description} Habitat: ${currentAnimal.habitat}, Danger Level: ${currentAnimal.dangerLevel}, Rarity: ${currentAnimal.rarity}. User question: ${question}`;
      }

      let sid = sessionId;
      if (!sid) {
        const startRes = await api.post(`/api/ai/start`);
        sid = startRes.data.sessionId;
        setSessionId(sid);
      }

      const res = await api.post(`/api/ai/ask/${sid}`, { questionAboutNature: finalQuestion });
      let answerText = res.data.answer || "No answer received";

      const formattedAnswer = answerText
        .split("\n")
        .filter(Boolean)
        .map((line, idx) => {
          let trimmed = line.trim().replace(/\*\*/g, "");
          if (/^(Danger Level|Habitat|Rarity|Classification|Height|Weight|Diet|Social Structure|Conservation Status|Interesting Fact)/i.test(trimmed)) {
            return <p key={idx} className="ai-highlight">{trimmed}</p>;
          }
          return <p key={idx}>{trimmed}</p>;
        });

      setMessages((prev) => [...prev, { type: "bot", content: formattedAnswer }]);
      setQuestion("");
    } catch (err) {
      alert("Failed to get answer: " + (err.response?.data?.message || err.message));
    } finally {
      setIsAsking(false);
    }
  };

  const sendFeedback = async () => {
    if (!sessionId) return;
    try {
      await api.post(`/api/ai/feedback/${sessionId}`, { rating, comment });
      setFeedbackOpen(false);
      alert("Feedback sent!");
      setComment("");
      setRating(5);
    } catch (err) {
      alert("Feedback failed: " + (err.response?.data?.message || err.message));
    }
  };

  // Icons
  const UploadIcon = (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
      <polyline points="17 8 12 3 7 8" />
      <line x1="12" y1="3" x2="12" y2="15" />
    </svg>
  );

  const SendIcon = (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <line x1="22" y1="2" x2="11" y2="13" />
      <polygon points="22 2 15 22 11 13 2 9 22 2" />
    </svg>
  );

  const SparkleIcon = (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M12 2L15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2z" />
    </svg>
  );

  const FeedbackIcon = (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M21 11.5a8.38 8.38 0 0 1-.9 3.8 8.5 8.5 0 0 1-7.6 4.7 8.38 8.38 0 0 1-3.8-.9L3 21l1.9-5.7a8.38 8.38 0 0 1-.9-3.8 8.5 8.5 0 0 1 4.7-7.6 8.38 8.38 0 0 1 3.8-.9h.5a8.48 8.48 0 0 1 8 8v.5z" />
    </svg>
  );

  return (
    <div className="ai-page-wrapper">
      {/* Random Bird Animation */}
      <div className="eagle-bg">
        {/* Clouds layer (behind birds) */}
        {clouds.map(cloud => (
          <div
            key={`cloud-${cloud.id}`}
            className={`cloud ${cloud.fromLeft ? 'drift-left-to-right' : 'drift-right-to-left'}`}
            style={{
              width: cloud.size.width * cloud.scale,
              height: cloud.size.height * cloud.scale,
              top: `${cloud.topPosition}%`,
              '--drift-duration': `${cloud.driftDuration}s`,
              opacity: cloud.opacity,
              zIndex: cloud.zIndex,
            }}
          >
            <img src={cloud.image} alt="" />
          </div>
        ))}
        {/* Birds layer (in front of clouds) */}
        {birds.map(bird => (
          <div
            key={bird.id}
            className={`bird ${bird.fromLeft ? 'fly-left-to-right' : 'fly-right-to-left'}`}
            style={{
              width: bird.size.width,
              height: bird.size.height,
              top: `${bird.topPosition}%`,
              '--flight-duration': `${bird.flightDuration}s`,
              '--flap-speed': `${bird.flapSpeed}s`,
              '--wobble': `${bird.wobble}px`,
            }}
          >
            <img src={eagleA} alt="" className="frame-a" />
            <img src={eagleB} alt="" className="frame-b" />
            <img src={eagleC} alt="" className="frame-c" />
          </div>
        ))}
      </div>

      <Header />

      <main className="ai-main">
        <div className="ai-container">
          {/* Left Panel - Upload & Controls */}
          <div className="ai-sidebar">
            <div className="ai-sidebar-header">
              <div className="ai-sidebar-icon">
                {SparkleIcon}
              </div>
              <h2>AI Wildlife Assistant</h2>
              <p>Upload an image to identify wildlife or ask questions about nature</p>
            </div>

            {sessionId && (
              <div className="ai-session-badge">
                <span className="ai-session-dot"></span>
                Session Active
              </div>
            )}

            {currentAnimal && (
              <div className="ai-current-animal">
                <span className="ai-animal-icon">🦁</span>
                <span>Discussing: <strong>{currentAnimal.name}</strong></span>
              </div>
            )}

            {/* Drop Zone */}
            <label className="ai-label" htmlFor="recognizer-select">Recognition Model</label>
            <select
              id="recognizer-select"
              className="ai-textarea"
              value={recognizer}
              onChange={(e) => setRecognizer(e.target.value)}
            >
              <option value="huggingface">Hugging Face</option>
              <option value="animaldetect">AnimalDetect</option>
            </select>

            <div 
              className={`ai-dropzone ${isDragOver ? 'drag-over' : ''} ${file ? 'has-file' : ''}`}
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onDrop={handleDrop}
              onClick={() => fileInputRef.current?.click()}
            >
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                onChange={(e) => setFile(e.target.files[0])}
                style={{ display: 'none' }}
              />
              {file ? (
                <div className="ai-file-preview">
                  <img src={URL.createObjectURL(file)} alt="Preview" />
                  <span className="ai-file-name">{file.name}</span>
                </div>
              ) : (
                <>
                  <div className="ai-dropzone-icon">{UploadIcon}</div>
                  <p className="ai-dropzone-text">Drag & drop an image here</p>
                  <p className="ai-dropzone-hint">or click to browse</p>
                </>
              )}
            </div>

            <button 
              className="ai-btn ai-btn-primary"
              onClick={analyze} 
              disabled={!file || isAnalyzing}
            >
              {isAnalyzing ? (
                <>
                  <span className="ai-spinner"></span>
                  Analyzing...
                </>
              ) : (
                <>
                  {SparkleIcon}
                  Analyze Image
                </>
              )}
            </button>

            <button 
              className="ai-btn ai-btn-secondary"
              onClick={() => setFeedbackOpen(true)} 
              disabled={!sessionId}
            >
              {FeedbackIcon}
              Give Feedback
            </button>

            <button 
              className="ai-btn ai-btn-outline"
              onClick={() => navigate("/")}
            >
              ← Back to Home
            </button>
          </div>

          {/* Right Panel - Chat */}
          <div className="ai-chat-panel">
            <div className="ai-chat-header">
              <h3>Chat with AI</h3>
              <p>Ask anything about wildlife and nature</p>
            </div>

            <div className="ai-chat-messages" ref={chatRef}>
              {messages.length === 0 ? (
                <div className="ai-chat-empty">
                  <div className="ai-chat-empty-icon">🌿</div>
                  <h4>Start a Conversation</h4>
                  <p>Upload an image to identify wildlife or ask a question about nature below</p>
                </div>
              ) : (
                messages.map((msg, idx) => (
                  <div key={idx} className={`ai-message ${msg.type}`}>
                    {msg.type === 'bot' && (
                      <div className="ai-message-avatar">🤖</div>
                    )}
                    <div className="ai-message-content">
                      {msg.isImage && msg.fileUrl && (
                        <img src={msg.fileUrl} alt="Uploaded" className="ai-message-image" />
                      )}
                      <div className="ai-message-text">{msg.content}</div>
                    </div>
                    {msg.type === 'user' && (
                      <div className="ai-message-avatar user">👤</div>
                    )}
                  </div>
                ))
              )}
            </div>

            <div className="ai-chat-input">
              <input
                type="text"
                placeholder="Ask about wildlife, habitats, conservation..."
                value={question}
                onChange={(e) => setQuestion(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && ask()}
                disabled={isAsking}
              />
              <button 
                className="ai-send-btn"
                onClick={ask}
                disabled={!question.trim() || isAsking}
              >
                {isAsking ? <span className="ai-spinner small"></span> : SendIcon}
              </button>
            </div>
          </div>
        </div>
      </main>

      <Footer />

      {/* Feedback Modal */}
      {feedbackOpen && (
        <div className="ai-modal-overlay" onClick={() => setFeedbackOpen(false)}>
          <div className="ai-modal" onClick={(e) => e.stopPropagation()}>
            <button className="ai-modal-close" onClick={() => setFeedbackOpen(false)}>×</button>
            
            <div className="ai-modal-header">
              <div className="ai-modal-icon">{FeedbackIcon}</div>
              <h3>Share Your Feedback</h3>
              <p>Help us improve the AI Wildlife Assistant</p>
            </div>

            <div className="ai-modal-body">
              <label className="ai-label">Rating</label>
              <div className="ai-rating">
                {[1, 2, 3, 4, 5].map((star) => (
                  <button
                    key={star}
                    className={`ai-rating-star ${rating >= star ? 'active' : ''}`}
                    onClick={() => setRating(star)}
                  >
                    ⭐
                  </button>
                ))}
              </div>

              <label className="ai-label">Comment (Optional)</label>
              <textarea
                className="ai-textarea"
                placeholder="Tell us about your experience..."
                value={comment}
                onChange={(e) => setComment(e.target.value)}
                rows={4}
              />
            </div>

            <button className="ai-btn ai-btn-primary" onClick={sendFeedback}>
              Submit Feedback
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
