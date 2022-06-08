## Running the Simulation
1. Go to `backend` folder and follow the instructions in README.md file.
2. I am using MacOS and I do not have access to a Windows and Linux machine. I have a script to launch as many instances as I want; however, the configuration may be different for other operating systems. Following instructions are based on MacOS environment.
3. Launch unity, import the `TermProject` project. Delete the current scene and add SampleScene from Scenes folder.
4. Build the project with unity. Let's assume name of the executable is `demo_1.app`. (Again, .app extension can be different in your OS)
5. Change your directory to `unity/TermProject` folder and give executable permissions to `launcher.sh`.
6.  Run `./launcher.sh 9` for launching 9 instances. You can provide any number you want.