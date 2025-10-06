# World Simulator

A Unity-based 3D world simulation engine featuring procedural terrain generation, advanced cloth physics, and custom shader implementations.
## ✨ Key Features

### Procedural Terrain Generation
- **Perlin Noise Implementation**: Custom compute shader based procedural noise generator for realistic terrain
- **Optimized Performance**: Achieved 1000+ FPS through compute shader optimization and vertex deformation for terrain

### 3D Cloth Simulation
- **Physics-Based Cloth**: Real-time 3D cloth simulation with gravity and constraint solving
- **Length Constraints**: Implemented structural and shear constraints for realistic fabric behavior

### Geometry Shader Based Grass Simulation
- **Geometry Grass Shader**: Procedurally generated grass using geometry shaders with custom tesselation
- **Custom Vertex Manipulation**: Advanced vertex displacement techniques with wind animations

## 📈 Development Timeline (DevLog)

### September 22, 2025
- ✅ **3D Cloth Simulation Complete**: Merged PR #9 - 3D cloth simulation with 22x22 mesh, gravity, and length constraints
- ✅ **CPU Implementation**: CPU-based cloth physics implementation finalized

### September 20, 2025
- ✅ **Merged PR #6 - Optimized mesh generation

### September 17, 2025
- ✅ **Water System**: Added terrain flattening for water bodies
- ✅ **Terrain Generation Complete**: Finalized Perlin noise-based terrain generation system

### September 16, 2025
- ✅ **Performance Optimization**: Achieved 1000+ FPS with optimized Perlin noise generation
- ✅ **Movement Controls**: Implemented first-person camera movement system

### September 11, 2025
- ✅ **Tiled Textures**: Created seamless tiled Perlin noise texture system

### September 7, 2025
- ✅ **Geometry Grass System**: Separated geometry shader-based grass rendering
- ✅ **Grass Shader**: Developed custom grass shader with wind effects

### September 3, 2025
- ✅ **Noise Texture Generator**: Merged PR #5 - Noise texture generator with quality visualization
- ✅ **Code Refactoring**: Separated Perlin noise vertices generator into modular component
- ✅ **Sphere Visualization**: Added sphere generation for Perlin noise visualization
- 📝 **README Update**: Initial documentation

### September 2, 2025
- ✅ **Terrain Prototype**: First working prototype of terrain generator
- ✅ **Project Setup**: Initial Unity project configuration and repository setup
- ✅ **Initial Commit**: Repository created

## 📫 Contact

Feel free to explore the code, raise issues, or reach out for collaboration opportunities!

---

**Note**: This is an active development project showcasing continuous learning and implementation of advanced graphics programming techniques.
