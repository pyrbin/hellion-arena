<a href="#"><img id="#logo" align="left" src="https://via.placeholder.com/100" width="150" height="150" style="margin-right: 15px;"></a>

## Celestial Arena (wip name)

**Celestial Arena** is a **turn-based multiplayer game** based on the Arena mode of **Divinity: Original Sin 2** made in **Unity DOTS**.

<br>

#### Table of Contents

- [Introduction](#Introduction)
- [Resources](#Resources)
  - [DOTS](#DOTS)
  - [Physics](#Physics)
  - [Netcode](#Netcode)

## Introduction

TODO

## Resources

- [Code Monkey (good tutorials)](https://www.youtube.com/channel/UCFK6NCbuCIVzA6Yj1G_ZqCg/videos)
- [DOTS Sample (netcode + dots + physics)](https://github.com/Unity-Technologies/DOTSSample)
- [Turn-based game prototype using ECS ](https://www.youtube.com/watch?v=mL4qrt-15TE)

### DOTS

#### Guidelines

- Systems should only have simple & definable tasks (Keep It Simple Stupid).
- If components always or mostly are used together they should probably be combined.
- Use keyword `in` for readonly components & `ref` for mutable components.
- Use `.Run()` for systems with inexpensive update & `.Schedule(handle)` for "heavy" systems.
- Use `UpdateBefore[typeof(/* system */)]` etc to define ordering of systems.

#### Editor

- `Assets > Create > DOTS` - Template code.
- `Window > Analysis > Entity Debugger` - DOTS debugger window.

#### Links

- 📋 [Documentation](https://docs.unity3d.com/Packages/com.unity.entities@0.7/manual/ecs_core.html)
- 🎓Tutorials/Samples
  - [Pong game](https://www.youtube.com/watch?v=a9AUXNFBWt4)
  - [DOTS-training-samples](https://github.com/Unity-Technologies/DOTS-training-samples)

### Physics

#### Navigation

- [DOTS NavMesh](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/Packages/com.reese.nav/README.md)
- [UnityAStarNavigation](https://github.com/jeffvella/UnityAStarNavigation)

#### Links

- 📋 [Documentation](https://docs.unity3d.com/Packages/com.unity.physics@0.2/manual/index.html)
- 🎓Tutorials/Samples
  - [UnityPhysicsSamples](https://github.com/Unity-Technologies/EntityComponentSystemSamples/tree/master/UnityPhysicsSamples)
  - [Getting Started Physics](https://www.youtube.com/watch?v=B3SFWm9gkL8)
  - [Havok video](https://www.youtube.com/watch?v=EJgB2Q5URvY)

### Netcode

#### Links

- 📋 [Documentation](https://docs.unity3d.com/Packages/com.unity.netcode@0.0/manual/index.html)
- 🎓Tutorials/Samples
  - [Introduction to NetCode](https://www.youtube.com/watch?v=P_-FoJuaYOI)

## Developers

<table>
  <tbody>
    <tr>
      <td align="center" valign="top">
        <a href="https://github.com/pyrbin">
            <img width="100" height="100" src="https://github.com/pyrbin.png?s=100">
            <br>
            pyrbin
        </a>
      </td>
      <td align="center" valign="top" style="padding-right:10px">
        <a href="https://github.com/smultronbusken">
            <img width="100" height="100" src="https://github.com/smultronbusken.png?s=100">
            <br>
            smultron
        </a>
      </td>
    </tr>
  </tbody>
</table>
