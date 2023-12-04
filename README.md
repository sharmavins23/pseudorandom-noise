# Pseudorandom Noise

This project is a follow-along of
[Jasper Flick's Catlike Coding](https://catlikecoding.com/unity/tutorials/)
tutorial on
[pseudorandom noise](https://catlikecoding.com/unity/tutorials/pseudorandom-noise).

Notably, this project was created via URP's default project, which (seemingly)
adds some form of global illumination and bloom. I think it looks fairly good,
so I'll keep it.

## Part 1 - [Hashing](https://catlikecoding.com/unity/tutorials/pseudorandom-noise/hashing/)

This part entails creating a hashing function to create pseudorandom noise.
We'll visualize it on a grid of procedurally generated cubes. I'll draw these
with an image of 123 by 123 pixels.

| Grid                                      | Notes                                                                                                   |
| ----------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| ![img](img/hashing/base.png)              | Our base grid.                                                                                          |
| ![img](img/hashing/repeat256.png)         | Making our grid repeat every 256 values gives some interesting (non-random) effects.                    |
| ![img](img/hashing/weyl.png)              | Adding Weyl sequencing makes a repeating pattern where the direction is based on the resolution.        |
| ![img](img/hashing/uvWeyl.png)            | Basing the Weyl sequence on the UV coordinates of the points breaks the pattern up, but adds this grid. |
| ![img](img/hashing/xxhash32C.png)         | XXHash32 is a fast hashing function using 5 prime numbers. This is the output with only 1/5.            |
| ![img](img/hashing/xxhash32rotate.png)    | After 'eating' data, XXHash32 will 'rotate' bit values to further permute.                              |
| ![img](img/hashing/xxhash32CDE.png)       | Here is the hashed output after involving primes C, D, and E (3/5 primes).                              |
| ![img](img/hashing/xxhash32avalanche.png) | 'Avalanching' is the final step - XORing and shifting a lot of random bits repetitively.                |
| ![img](img/hashing/scaleInvariance.gif)   | This hashing function demonstrates scale invariance as well.                                            |
| ![img](img/hashing/hashColors.png)        | Our previous visualization only showed 1/4 of the random bytes. Now, we can see 3/4ths with colors.     |
| ![img](img/hashing/variedOffset.png)      | Finally, varying the offset lets us show the final random byte. Nice!                                   |

## Part 2 - [Hashing Space](https://catlikecoding.com/unity/tutorials/pseudorandom-noise/hashing-space/)

Currently our hashing only works with two-dimensional non-transformed grids. We
can see this if we scale down our UV coordinates:

![img](img/hashing-space/uvquarter.png)

Applying a quarter-scale linear transformation causes the random values to
cluster, which is not usable. As such, let's fix this to be able to apply any
translation, rotation, and scaling. Like before, I'll summarize notes in a
table:

| Image                                     | Notes                                                                                            |
| ----------------------------------------- | ------------------------------------------------------------------------------------------------ |
| ![img](img/hashing-space/uvRotate.png)    | Adding support for transformation matrices allows us to rotate and scale our hashing.            |
| ![img](img/hashing-space/zSlice.png)      | We can even rotate across axes to see different slices of the hash's volume.                     |
| ![img](img/hashing-space/linearScale.png) | Moving this 3D object allows us to explore the global hash volume.                               |
| ![img](img/hashing-space/rotation.png)    | This 3D support requires we make our displacement based off normal vectors instead of Y vectors. |
| ![img](img/hashing-space/sphereMap.png)   | Since we have 3D support, we can map to all kinds of 3-dimensional objects. Here's a ball...     |
| ![img](img/hashing-space/torus.png)       | Here's a torus with high resolution. An interesting band appears on top.                         |
| ![img](img/hashing-space/star.png)        | I couldn't resist, and had to implement the spiral star from the graphs tutorial.                |
| ![img](img/hashing-space/pulsingthin.gif) | With a tiny amount of work, we can make the objects pulse!                                       |

# License TL;DR

This project is distributed under the MIT license. This is a paraphrasing of a
[short summary](https://tldrlegal.com/license/mit-license).

This license is a short, permissive software license. Basically, you can do
whatever you want with this software, as long as you include the original
copyright and license notice in any copy of this software/source.

## What you CAN do:

-   You may commercially use this project in any way, and profit off it or the
    code included in any way;
-   You may modify or make changes to this project in any way;
-   You may distribute this project, the compiled code, or its source in any
    way;
-   You may incorporate this work into something that has a more restrictive
    license in any way;
-   And you may use the work for private use.

## What you CANNOT do:

-   You may not hold me (the author) liable for anything that happens to this
    code as well as anything that this code accomplishes. The work is provided
    as-is.

## What you MUST do:

-   You must include the copyright notice in all copies or substantial uses of
    the work;
-   You must include the license notice in all copies or substantial uses of the
    work.

If you're feeling generous, give credit to me somewhere in your projects.
