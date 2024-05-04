# EnSUREd: A Sustainable Energy Management Tycoon  

![Screenshot of the PoC](Ensured_Menu.png)  

#### Context  
  In the scope of the [SWEET SURE: Sustainable and Resilient Energy for Switzerland](https://sweet-sure.ch/) project, which is funded by the Swiss Confederation and is a collaboration between various research institutions, we have been tasked by the Università della Svizzera Italiana (USI)'s Euler Institute to create a video game illustrating the difficult dicisions that must be made to achieve net zero carbon emissions by 2050. For this project we are a core team of 3 people for the video game itself (a developer, a game designer, and an artist), as well as one post-doc working on the energy grid simulation backend, a team of political scientists at the universtiy of Bern designing the policies, and their impact on the political landscape, that the player can put in place to mitigate climate shocks, as well as 3 projects leaders coordinating the various different teams.

#### Description  

  Throughout this turn-based game, the energy minister (a.k.a. the player) is confronted with a series of societal or environmental shocks which can disrupt the grid. In response to this, they can implement policies which will mitigate the impacts of these shocks all while impacting their political support amongst the people. Given that Switzerland is a Direct Democracy, the people's support is alwayd required to get anything done, so it's important to keep the political landscape of the country in mind when implementing new energy policies. Practically, the player works towards the country's energy transition by building new power-plants, retireing old ones, implementing energy policies, managing energy imports, as well as reacting to climate shocks.    


## Development  
This game is developed in C# using the [Godot mono](https://godotengine.org/) game engine. On this repository you will find all of the files required to run and contribute to our project. These include:  
 - `assets`: Which contains the art assets used for the visuals of our game.
 - `db`: Which contains all of the xml config and dynamic text files.
 - `scenes`: Which contains all of the godot scenes used to setup the various parts of our game.
 - `src`: Which contains all of the code that our game runs on.
 - `project.godot`: Which is the godot project file that needs to be opened to edit our project in the Godot editor.



## Inspiration  
Certain existing games can be used as inspiration to understand the goal of this project:  
  - [Power The Grid](https://claudioa.itch.io/power-the-grid): This is a similar style game, where the goal is to transition a local energy grid towards being able to decomission a coal plant. The art style and core plant management systems in our game were
  - [Frostpunk](https://www.frostpunkgame.com/): This game is a lot more gritty and bleak but uses a similar structure in terms of the political support system.

    
