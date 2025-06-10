# Build a better collider importer with Asset Processors
An example AssetProcessor that improves on Unity's default import collider generation. Read the original author's accompanying blog post [here](https://bronsonzgeb.com/index.php/2021/11/27/better-collider-generation-with-asset-processors/).

![Example](https://github.com/dgul3d/AutomaticColliderGeneration/assets/64034875/e996bfcf-91d7-42cc-bdd7-a908e100ac6b)

### Usage
You can enable/disable automatic collider generation from the `/Tools/Better Collider Generation` toggle.


This asset uses [Unreal Collider Prefixes](https://docs.unrealengine.com/4.26/en-US/WorkingWithContent/Importing/FBX/StaticMeshes/) for automatic collider generation on import.

The possible prefixes are:

- UBX_ - Box collider
- UCP_ - Capsule collider
- USP_ - Sphere collider
- UCX_ - Convex mesh collider
- UMC_ - Concave mesh collider. Notice - is not compatible with modern (v4, v5) Unreal collider importers.

Collider mesh renderer is being destroyed at import. If collider object has no rotation then collider component is being set to the parent object. If collider object has any rotations then collider component remains attached to the rotated child object. 

![Hierarchy](https://github.com/dgul3d/AutomaticColliderGeneration/assets/64034875/d612bf31-9ae8-44f4-8a53-8b0cd0e9ee66)

## Installation
### Option 1

Requires GIT installation

Window > Package Manager > + > Add package from git url > `https://github.com/dgul3d/AutomaticColliderGeneration.git`

### Option 2
Download as .zip > unpack anywhere in `/Assets/` folder of your project.
