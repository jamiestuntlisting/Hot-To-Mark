## URP Setup Instructions

Unity generates URP pipeline assets as binary ScriptableObjects that cannot be
created as text files. Follow these steps after opening the project in Unity:

### 1. Create URP Pipeline Asset
1. Right-click in `Assets/Settings/`
2. Select **Create > Rendering > URP Asset (with Universal Renderer)**
3. Name it `URPPipelineAsset`

### 2. Assign the Pipeline Asset
1. Go to **Edit > Project Settings > Graphics**
2. Set the **Scriptable Render Pipeline Settings** to the asset you just created

### 3. Configure URP for iOS Performance
Open the URPPipelineAsset and set:
- **Rendering > Render Scale**: 1.0
- **Quality > Anti Aliasing (MSAA)**: 2x
- **Quality > HDR**: OFF (for iOS performance)
- **Shadows > Max Distance**: 60
- **Shadows > Cascade Count**: 2
- **Shadows > Shadow Resolution**: 1024
- **Post Processing**: Enable if desired (bloom, color grading)

### 4. Quality Levels
1. Go to **Edit > Project Settings > Quality**
2. Set iOS default to **Medium**
3. The `PerformanceOptimizer.cs` script dynamically adjusts quality at runtime
