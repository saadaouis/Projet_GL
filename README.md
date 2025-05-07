# Project_GL
This repository contains the group-project of Seif, Achille, Vincent and Olaf for the software engineering bloc.

## UML diagrams
This part contains the UML diagrams of the save software.

#### Activity diagram
![activity diagram](ressources/activity_diagram.png)

#### Class diagram
![class diagram](ressources/class_diagram.png)

#### Sequence diagram
# Projet_GL test
Ce repository contient le projet en groupe de Saifallah, Achille, Vincent, Olaf et Seif pour le bloc genie logiciel en C#

```mermaid
  %%{init: {
  'theme': 'base',
  'themeVariables': {
    'primaryColor': '#f4f4f4',
    'primaryTextColor': '#000000',
    'primaryBorderColor': '#000000',
    'lineColor': '#000000',
    'secondaryColor': '#f0f0f0',
    'tertiaryColor': '#ffffff'
  }
}}%%

sequenceDiagram
    actor U as User
    participant I as IHM
    participant M as Main
    participant V as View
    participant C as Controller
    participant MC as Model Config
    participant MB as Model Backup
    participant S as System
    participant L as Logger
    
    rect rgba(245, 245, 250, 0.3)
    activate U
    
    rect rgba(220, 220, 255, 0.5)
    Note right of U: Initialization Phase
        U->>M: Start App
        activate M
        M->>C: initializationProcess()
        activate C
        C->>MC: fetchConfig()
        activate MC
        MC-->>C: Config not found
        C->>L: Log("Config not found", "ERROR")
        activate L
        deactivate L
        C->>V: initializationForm()
        activate V
        V->>I: Display the config form
        activate I 
        I->>U: Request Initial Config
        
        U-->>I: Configure Directories A-B & Language
        I-->>V: Completed Initial Config
        V-->>C: Completed initializationForm()

        C->>MC: setConfig(directoryA: string, directoryB: string, language: string)
        MC-->>C: Config Saved Successfully
        C->>L: Log("Config Saved", "INFO")
        activate L
        deactivate L
        C->>MC: loadConfig()
        MC-->>C: Config Loaded
        deactivate MC
        C->>L: Log("Config Loaded", "INFO")
        activate L
        deactivate L
    end

    rect rgba(246, 66, 105, 0.2)
    Note right of U: System Menu
    C->>V: displayMenu()
    V->>I: Display Option Menu
    I->>U: Request to choose an option from the menu
    U-->>I: Choose Option between 1-5
    I-->>V: Completed Option Menu
    V-->>C: displayMenu() Output
    C->>L: Log("Option " + option + " selected", "INFO")
    activate L
    deactivate L
    end

    alt Option 1
    rect rgba(146, 36, 200, 0.2)
    Note right of U: Option 1 : Download Project
    C->>V: showProjectList()
    V->>I: Display Project List
    I->>U: Request to choose a project
    U-->>I: Choose Between 1-5
    I-->>V: Selected Project n
    V-->>C: showProjectList() Output
    C->>L: Log("Project " + projectId + " selected", "INFO")
    activate L
    deactivate L
    C->>MB: fetchVersionList(projectId: int)
    activate MB
    MB-->>C: List of project versions
    deactivate MB
    C->>V: showVersionList(versions: List<Version>)
    V->>I: Display options
    I->>U: Request to choose a version
    U-->>I: Version n
    I-->>V: Version n selected
    V-->>C: showVersionList() Output
    C->>L: Log("Version " + versionId + " selected", "INFO")
    activate L
    deactivate L
    C->>MB: getVersion(projectId: int, versionId: int)
    activate MB
    MB-->>C: Beginning download of project
    C->>L: Log("Starting Download for project " + projectId + " version " + versionId, "INFO")
    activate L
    deactivate L
    MB->>S: Copy file from directory B to A
    activate S
    C->>V: displayMessage("Starting Download")
    V->>I: Display Starting Download
    I->>U: Starting Download
    
    S-->>MB: Finished copying file from B to A
    deactivate S
    MB-->>C: Download Finished
    deactivate MB
    C->>L: Log("Download for project " + projectId + " version " + versionId + " successful", "INFO")
    activate L
    deactivate L
    C->>V: displayMessage("Download Finished")
    V->>I: Display Download Finished
    I->>U: Download Finished
    end
    end

    alt Option 2
    rect rgba(146, 206, 200, 0.2)
    Note right of U: Option 2 : Save Project
    C->>V: showProjectList()
    V->>I: Display Project List
    I->>U: Request to choose a project
    U-->>I: Choose Between 1-5
    I-->>V: Selected Project n
    V-->>C: showProjectList() Output
    C->>L: Log("Project " + projectId + " selected", "INFO")
    activate L
    deactivate L
    C->>MB: createSave(projectId: int)
    activate MB
    MB-->>C: Beginning save of project
    C->>L: Log("Starting save for project " + projectId + " version N+1", "INFO")
    activate L
    deactivate L
    MB->>S: Copy file from directory A to B
    activate S
    C->>V: displayMessage("Saving...")
    V->>I: Display Saving
    I->>U: Saving...
    
    S-->>MB: Finished copying file from A to B
    deactivate S
    MB-->>C: Save Finished
    deactivate MB
    C->>L: Log("Save for project " + projectId + " version N+1 successful", "INFO")
    activate L
    deactivate L
    C->>V: displayMessage("Finished Saving")
    V->>I: Display Finished Saving
    I->>U: Finished Saving
    end
    end

    alt Option 3
    rect rgba(226, 226, 226, 0.2)
    Note right of U: Option 3 : Enable Auto-Saving
    C->>V: showProjectList()
    V->>I: Display Project List
    I->>U: Select one or multiple projects
    U-->>I: Choose Projects (Toggle)
    I-->>V: Selected Projects
    V-->>C: Project List Selected
    C->>L: Log("Selected Project List: " + projectIds, "INFO")
    activate L
    deactivate L
    
    C->>MB: activateAutoSave(projects: List<Project>)
    activate MB
    MB-->>C: Auto-Save enabled for n projects
    C->>V: displayMessage("Auto-Save enabled for " + n + " projects")
    V->>I: Display "Auto-Save enabled for " + n + " projects"
    I->>U: Auto-Save activated for n projects
    
    loop Auto-Saving (every 15 minutes)
        MB-->>MB: Wait 15 minutes
        loop For each selected project
            MB->>S: Compare Directory A with Latest Major Version
            S-->>MB: Return Differences
            MB->>S: Save New Version (v.x.n+1) in Project Directory
            MB-->>C: Project n successfully updated
            C->>L: Log("Project " + projectId + " successfully updated", "INFO")
        end
        MB-->>C: Successful 15mn update loop
        C->>L: Log("All projects successfully updated", "INFO")
    end
    
    deactivate MB
    end
    end

    alt Option 4
    rect rgba(50, 50, 50, 0.2)
    Note right of U: Option 4 : Config Modification
    C->>V: showConfigForm()
    V->>I: Display Config Form
    I->>U: Request to modify config
    U-->>I: Modify Directories A-B & Language
    I-->>V: Completed Config Modification
    V-->>C: showConfigForm() Output
    C->>L: Log("Config modification requested", "INFO")
    activate L
    deactivate L
    C->>MC: updateConfig(directoryA: string, directoryB: string, language: string)
    activate MC
    MC-->>C: Config Updated Successfully
    deactivate MC
    C->>L: Log("Config updated successfully", "INFO")
    activate L
    deactivate L
    C->>V: displayMessage("Config updated successfully")
    V->>I: Display Config Updated
    I->>U: Config Updated Successfully
    end
    end

    alt Option 5
    rect rgba(240, 235, 80, 0.2)
    Note right of U: Option 5 : Quit Program
    C->>V: confirmQuit()
    V->>I: Display Quit Confirmation
    I->>U: Request confirmation to quit
    U-->>I: Confirm Quit
    I-->>V: Quit Confirmed
    V-->>C: confirmQuit() Output
    deactivate V
    C->>L: Log("Program termination requested", "INFO")
    activate L
    deactivate L
    C-->>M: return
    deactivate C
    M->>I: Close Application
    deactivate I
    deactivate M
    end
    end

    deactivate U
    end

```
