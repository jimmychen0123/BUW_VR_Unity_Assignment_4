# Bauhaus University Weimar/ VR Unity Final Project
## Topic: Getting there together (Weissker et al. 2020)

Implement a Multi-Ray Jumping and Positioning technique as described by Weissker
et al. with a simulated second user as the passenger in the virtual environment.
 
Reference: Weissker, T., Bimberg, P., Froehlich, B. (2020). Getting There Together: Group
Navigation in Distributed Virtual Environments. IEEE transactions on visualization and computer
graphics, 26(5), 1860-1870.

## Implementation
The requirements of group navigation should foster the awareness of ongoing navigation activities and facilitate the presdictability of their consequences for navigator and all passengers.

From passenger's perspectives: 
* Passengers should receive notification mechanism to raise attention when the navigator plans a jump so the teleportation does not come in unexpected way.
* Offering a clear visible indication of the target location for all participants.

From navigator's perspectives:
* Since the navigator has responsibility to move the group, the navigator need to have awareness of current spatial configuration of users in the workspace.
* Offering a clear visible indication of the target location for navigator.

Four phases that involve in group navigation for distributed users:
 * Forming: group creation and joining mechanisms. 
 * Norming: responsibility for navigator or passengers while navigating together.
 * Performing: group navigation technique
 * Adjouring: Group termination mechanisms.

In this project, we are focusing on only norming and performing phases.

## How to realise the group navigation for more than two users (without adding an individual target ray for each user)? 

Our project implements the Multi-Ray Jumping and Positioning technique described by Weissker et al. with a simulated second user as the passenger in the virtual environment. Comprehensible group jumping and positioning could also be realized for more than two users by using predefined formations. The navigator could select their desired target position using the trigger button and choose the group’s formation by pressing the grip button to browse different formations (such as a circle, queue, etc.). These different formations would allow the navigator to easily adjust the group formation based on the layout of the environment and the goals of the group.

![Untitled Notebook (1)-2](https://user-images.githubusercontent.com/39960241/115073133-3a2c6c80-9ef8-11eb-8098-60a80d4b24a0.jpg)
![Untitled Notebook (1)-3](https://user-images.githubusercontent.com/39960241/115073041-34cf2200-9ef8-11eb-85b3-a430f0533d0d.jpg)


## What are the advantages and disadvantages of group jumping as opposed to group steering?

Group jumping induces less motion sickness than group steering, however, it results in less spatial awareness during and after travel compared to group steering. Group steering is disadvantageous because it induces more motion sickness than group jumping, but it results in better spatial awareness during and after travel. The problems of collision can occur with both types of group navigation. These collisions might be better predicted in group jumping when preview avatars are used to view the final position of the group before navigating. Group steering can use augmented group steering techniques such as detouring, distortion, etc. to avoid these collisions, but these techniques can negatively impact users’ spatial awareness in the environment.



