# Frever

A Unity-based mobile client for a [Frever](https://github.com/FriendFactory/frever-open) project. 

## Table Of Contents

- [Getting Started](#getting-started)
- [Dependencies](#dependencies)
  - [Plugins](#plugins)
  - [External Services](#external-services)
- [Structure](#structure)
- [License](#license)
- [Support](#support)
- [Contributing](#contributing)

## Getting Started

**Requirements:**

- Unity version: `2022.3.28f1`

1. Download and install Unity Editor (with iOS and/or Android build support modules)
2. Clone repository
3. Initialize `git lfs` by running `git lfs install` in the project directory, then pull LFS files with `git lfs pull`
4. Open the project from the Unity Hub or Unity Editor
5. Resolve dependencies

*Note: This project has internal and external dependencies that must be properly configured. Ensure the backend services are up and running before testing the application. The project relies on external services connectivity for full functionality. Some features may not work correctly without proper backend configuration and network access to required external services.*

## Dependencies

- up-and-running [Frever Backend](https://github.com/FriendFactory/frever-open-backend)

### Plugins

- [3D Visualizer Spectrum Vu Meter](https://assetstore.unity.com/packages/tools/audio/3d-visualizer-spectrum-vu-meter-100995) 2
- [Amplitude Analytics](https://assetstore.unity.com/packages/tools/audio/amplitude-for-webgl-111277) 1.2.4
- [Animated GIF Player](https://discussions.unity.com/t/released-animated-gif-player/657636)
- [Atmospheric Height Fog](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/atmospheric-height-fog-optimized-fog-for-consoles-mobile-and-vr-143825) (upd: 14/05/2020)
- [AVPro Movie Capture](https://assetstore.unity.com/packages/tools/video/avpro-movie-capture-desktop-edition-221914) 5.2.4
- [AVPro Video](https://assetstore.unity.com/packages/tools/video/avpro-video-v3-core-edition-278893) 2.6.7
- [Capture The Gif](https://assetstore.unity.com/packages/tools/video/capture-the-gif-15673) 2.0.0
- [Console Pro](https://assetstore.unity.com/packages/tools/utilities/console-enhanced-pro-11521) 3.3.6
- [DOTWeen Pro](https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416) 1.0.381
- [Dynamic Bone](https://assetstore.unity.com/packages/tools/animation/dynamic-bone-16743)
- [Enhanced Scroller](https://assetstore.unity.com/packages/tools/gui/enhancedscroller-36378) 2.33.0
- [Essential Kit](https://assetstore.unity.com/packages/tools/integration/essential-kit-v3-iap-leaderboards-cloud-save-notifications-galle-301752) 2.0.0
- [Fingers](https://assetstore.unity.com/packages/tools/input-management/fingers-touch-gestures-for-unity-41076) 3.0.14
- [Flow Layout Group](https://assetstore.unity.com/packages/tools/gui/flow-layout-group-233675) 1.0.0
- [I2 Localization](https://assetstore.unity.com/packages/tools/localization/i2-localization-14884) 2.8.22 f3
- [Living Particles](https://assetstore.unity.com/packages/vfx/particles/living-particles-105817) (upd: 28/08/2020)
- [Lumos](https://github.com/rahul-anand/Lumos)
- [Native Gallery](https://assetstore.unity.com/packages/tools/integration/native-gallery-for-android-ios-112630)
- [Native Share](https://assetstore.unity.com/packages/tools/integration/native-share-for-android-ios-112731)
- [Nature Renderer 2022](https://assetstore.unity.com/packages/slug/nature-renderer-2022-266664) 2022.0.3
- [Nature Renderer 6・Free](https://assetstore.unity.com/packages/tools/terrain/nature-renderer-6-free-285961)
- [Nature Shaders 2021](https://assetstore.unity.com/packages/vfx/shaders/nature-shaders-2021-221248) 2021.1.2
- [Odin Inspector](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041) 3.0.12
- [OSA](https://assetstore.unity.com/packages/tools/gui/optimized-scrollview-adapter-68436) 7.2.1
- [Particle Plexus](https://assetstore.unity.com/packages/vfx/particles/particle-plexus-36381)
- [Procedural UI](https://assetstore.unity.com/packages/tools/gui/procedural-ui-193375)
- [QFSW Quantum Console](https://assetstore.unity.com/packages/tools/utilities/quantum-console-211046) 2.6.6
- [R.A.M - River Auto Material](https://assetstore.unity.com/packages/tools/terrain/r-a-m-river-auto-material-101205) (upd: 06/05/2020)
- [Rhythm Visualizator Pro](https://assetstore.unity.com/packages/tools/audio/rhythm-visualizator-pro-14437) (upd: 12/10/2020)
- [Spiral Generator](https://assetstore.unity.com/packages/tools/particles-effects/spiral-generator-2986) 2.2.1
- [Stan's Assets Foundation](https://github.com/StansAssets/com.stansassets.foundation) 1.0.19
- [Stan's Assets Mobile](https://github.com/StansAssets/com.stansassets.mobile) 0.0.3
- [Stan's Assets Plugins Dev Kit](https://github.com/StansAssets/com.stansassets.plugins-dev-kit) 1.0.2
- [Stan's Assets Xcode Project](https://github.com/StansAssets/com.stansassets.xcode-project) 0.0.1
- [Tunnel FX 2](https://assetstore.unity.com/packages/vfx/shaders/tunnel-fx-2-86544) (upd: 17/09/2020)
- [UI Soft Mask](https://assetstore.unity.com/packages/tools/ui/soft-mask-for-ugui-41839) 1.0.0
- [Ultimate Mobile Pro](https://assetstore.unity.com/packages/tools/integration/ultimate-mobile-pro-130345)
- [Unity Detect Headset](https://github.com/DaVikingCode/UnityDetectHeadset)
- [URP Car Paint Shader & Effects](https://assetstore.unity.com/packages/vfx/shaders/urp-car-paint-shader-effects-263074) 2.0.0
- [URP Mirror Shader](https://assetstore.unity.com/packages/vfx/shaders/urp-mirror-shaders-ar-vr-ready-135215#content)
- [Volumetric Fog & Mist 2](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/volumetric-fog-mist-2-162694) (upd: 29/06/2020)
- [Volumetric Light Beam](https://assetstore.unity.com/packages/vfx/shaders/volumetric-light-beam-99888) (upd: 12/12/2019)

*Note: plugins listed below were integrated based on specific project requirements as they emerged during development. This iterative approach has resulted in some overlapping functionality between certain plugins.*

### External Services

- [Amplitude](https://amplitude.com/) (optional)
- [Sentry](https://sentry.io/welcome/) (optional)
- [Apps Flyer](https://www.appsflyer.com/) (optional)

## Structure

## License

This project is licensed under the [MIT License](LICENSE).

Please note that the Software may include references to “Frever” and/or “Ixia” and that such terms may be subject to trademark or other intellectual property rights, why it is recommended to remove any such references before distributing the Software.

## Support

This repository is provided as-is, with no active support or maintenance. For inquiries related to the open source project, please contact:

**📧 admin@frever.com**

## Contributing

We welcome forks and reuse! While the platform is no longer maintained by the original team, we hope it serves as a useful resource for:

- Generative media research and tooling
- Mobile-first creative platform development
- AI-enhanced prompt and content flows

Please open issues or pull requests on individual repos if you want to share fixes or improvements.
