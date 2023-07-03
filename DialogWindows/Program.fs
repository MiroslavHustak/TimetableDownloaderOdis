module BrowserDialogWindow

open System
open Microsoft.Win32

let openFolderBrowserDialog() = //I

    try 
        let folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog() 

        folderBrowserDialog.SelectedPath <- Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        folderBrowserDialog.Description <- "Select a folder"

        let result = folderBrowserDialog.ShowDialog()
        
        match result = System.Windows.Forms.DialogResult.OK with
        | true  -> folderBrowserDialog.SelectedPath, false
        | false -> String.Empty, true         
    with
    | ex -> (string ex), true
               

    
