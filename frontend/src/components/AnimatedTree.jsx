import { useRef } from 'react';

// Import all tree frames
import treefront1 from '../images/Nature/forest/Fronttree/treefront.svg';

/**
 * Animated Tree component - now static (no animation)
 * Always displays the static treefront.svg 
 * Falling leaves animation is handled by parent (SearchPage)
 */
export default function AnimatedTree() {
  const containerRef = useRef(null);

  return (
    <div ref={containerRef} className="animated-tree-container">
      <img 
        src={treefront1} 
        alt="" 
        className="tree-frame static-tree"
        style={{ opacity: 1 }}
      />
    </div>
  );
}
