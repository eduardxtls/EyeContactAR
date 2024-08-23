# AR Eye Contact Recognition for Individuals with Visual Impairment

## Project Overview

This project, which is part of a Bachelor's Thesis at the University of Stuttgart, uses the Microsoft Mixed Reality Toolkit (MRTK) and HoloLens 2 devices to create an AR application that helps individuals with visual impairment perceive interest through eye contact coming from a potential observer. In this work, we implemented two visual cues and an audio one. The observer uses eye tracking to determine interest, providing cues to the individual with visual impairment.

## Goals

- **Eye Tracking:** Utilize HoloLens 2's eye tracking to monitor the observer's gaze.
- **Interest Detection:** Detect observer interest when their gaze is on the visually impaired player.
- **Feedback Mechanism:** Provide real-time feedback to the visually impaired player via visual audio cues.

## Features

1. **Calibration:** Accurate eye tracking calibration.
2. **Gaze Recognition:** Identify the visually impaired player as the gaze target.
3. **Interest Algorithm:** Determine interest from gaze data.
4. **Real-Time Feedback:** Immediate cues for the visually impaired player.

The included cues are:

- a pulsing animation that covers the whole screen and notifies the user about a person watching them for longer that 8 seconds
- a beam that connects the two persons, which helps the individual with visual impairment locate and follow the observer
- a spatialized sonar sound that also helps locate the observer, helpful for various dark environments

## Requirements

- **Hardware:** HoloLens 2, feedback device (audio and/or visual).
- **Software:** Unity 3D, MRTK, Visual Studio.

## Implementation

1. **Setup HoloLens 2 with MRTK.**
2. **Calibrate Eye Tracking.**
3. **Develop Gaze Target Recognition.**
4. **Create Interest Detection Logic.**
5. **Integrate Feedback Mechanism and Audiovisual Cues.**
6. **Test and Iterate.**

## Use Case

- **Social Interactions**


## Future Enhancements

- Multi-observer support
- Advanced feedback systems
- Computer vision for on-device eye contact recognition through cameras
