# SpatialGrid
Fast Spatial Hashing for Unity GameObjects with zero garbage allocation

![image](https://github.com/user-attachments/assets/55a81bb4-9e31-45ac-870b-5c67a1cfad98)


Performance on a i7-14700KF @ 3.40 GHz

Tested on a build, mean results over 1000 frames:

- 1,000,000 Queries:
  	- Radius covering 9 cells - 0.98s
	- Radius covering 25 cells - 1.11s
	- Radius covering 49 cells - 1.19s
	- Radius covering 81 cells - 1.34s
	- Radius covering 121 cells - 1.49s
	
- 1,000,000 Insertions - 0.78s
- 1,000,000 Removals - 1.89s
