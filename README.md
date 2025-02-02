Overview

The GCode Polar Converter Tool is a software utility designed to convert G-code files from Cartesian coordinates (X, Y, Z) to polar coordinates (θ, R, Z). This tool is specifically tailored for 3D printers with a traditional X-axis (polar) and a rotating build plate (Y-axis as rotational). The tool also includes features to optimize movements for polar coordinates, support multiple print heads, and simulate the printing process.
Key Features:

    Cartesian to Polar Conversion:

        Converts standard G-code (Cartesian) to polar coordinates for printers with a rotating build plate.

        Handles G0 (rapid movement) and G1 (linear movement) commands.

    Movement Optimization:

        Optimizes G-code for polar coordinates to minimize unnecessary rotations and improve print quality.

    Multiple Print Heads:

        Supports multiple extruders with optimized geometry for simultaneous printing. (not implemented)

    Print Simulation:

        Provides a visualization tool to simulate the printing process in polar coordinates. (under development)

How It Works
1. Cartesian to Polar Conversion

The tool converts Cartesian coordinates (X, Y) to polar coordinates (θ, R) using the following formulas:

    Radius (R): R=X2+Y2R=X2+Y2

    ​

    Angle (θ): θ=arctan⁡(YX)×(ThetaFactor2π)θ=arctan(XY​)×(2πThetaFactor​)

The Z-axis remains unchanged.
2. Centering the Geometry

Before conversion, the tool calculates the centroid (geometric center) of the model and translates the G-code so that the centroid is at the origin (0, 0). This ensures that the rotation is centered.
3. Movement Optimization

The tool optimizes G-code for polar coordinates by:

    Minimizing unnecessary rotations of the build plate.

    Ensuring smooth transitions between movements.

4. Simultaneous Multiple Print Heads

This is one of the objectives of this software, but this feature has not yet been implemented. For printers with a polar coordinate table (X-axis), the idea is to have multiple independent Y-axes where the extrusion is divided between the extruders in parallel in order to optimize the printing time of two extruders sharing the movement of the same table, but taking advantage of the polar motion system in the simultaneous printing of the same piece."
5. Print Simulation

The tool includes a visualization feature to simulate the printing process in polar coordinates. This helps users verify the converted G-code before printing.
