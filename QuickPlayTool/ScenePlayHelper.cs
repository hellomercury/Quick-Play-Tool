﻿/*
Copyright (c) 2018 S. Tarık Çetin

This project is licensed under the terms of the MIT license.
Refer to the LICENCE file in the root folder of the project.
*/

using UnityEditor;
using UnityEditor.SceneManagement;

namespace QuickPlayTool
{
    public static class ScenePlayHelper
    {
        private static string _sceneToPlay;

        public static void PlayScene(string scenePath)
        {
            _sceneToPlay = scenePath;

            if (!EditorApplication.isPlaying)
            {
                // Delay the call to let all interfaces finish what they are doing.
                // Unity plays delayCall subscriber functions only once, so no unsubscribe needed.
                EditorApplication.delayCall += () => { _PlayScene(_sceneToPlay); };
            }
            else
            {
                EditorApplication.isPaused = true;

                // Ask if user wants to restart the play mode.
                var dialogAnswer = EditorUtility.DisplayDialogComplex(
                    "Already in play mode",
                    "Editor is already in play mode. \n\n Selected scene: " + scenePath,
                    "Play selected scene",
                    "Do nothing",
                    "Stop play");

                switch (dialogAnswer)
                {
                    case 0: // Play selected scene
                        EditorApplication.playModeStateChanged += _PlayScene_IfInEditMode_AndUnsubscribe;
                        break;

                    case 1: // Do nothing
                        EditorApplication.isPaused = false;
                        return;
                }

                EditorApplication.isPlaying = false;
            }
        }

        /// <summary>
        /// (If the <paramref name="stateChange"/> is <see cref="PlayModeStateChange.EnteredEditMode"/>) Calls
        /// <see cref="_PlayScene(string)"/> with <see cref="_sceneToPlay"/>, then unsubscribes itself from
        /// <see cref="EditorApplication.playModeStateChanged"/> event of <see cref="EditorApplication"/>
        /// </summary>
        private static void _PlayScene_IfInEditMode_AndUnsubscribe(PlayModeStateChange stateChange)
        {
            if (stateChange != PlayModeStateChange.EnteredEditMode) return;

            _PlayScene(_sceneToPlay);
            EditorApplication.playModeStateChanged -= _PlayScene_IfInEditMode_AndUnsubscribe;
        }

        /// <summary>
        /// Saves scenes and then (if users doesn't press cancel on save dialog) plays the scene with
        /// <paramref name="scenePath"/>
        /// </summary>
        private static void _PlayScene(string scenePath)
        {
            // SaveCurrentModifiedScenesIfUserWantsTo returns false if user presses "cancel" on save dialog.
            // I should not proceed if that is the case.
            var didSave = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            if (!didSave) return;

            EditorSceneManager.OpenScene(scenePath);
            EditorApplication.isPlaying = true;
        }
    }
}