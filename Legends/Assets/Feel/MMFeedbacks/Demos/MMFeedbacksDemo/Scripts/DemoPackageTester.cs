using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This class, meant to be used in MMFeedbacks demos, will check for requirements, and output an
    /// error message if necessary.
    /// </summary>
    public class DemoPackageTester : MonoBehaviour
    {
        [MMFInformation("This component is only used to display an error in the console in case dependencies for this demo haven't been installed. You can safely remove it if you want, and typically you wouldn't want to keep that in your own game.", MMFInformationAttribute.InformationType.Warning, false)]
        /// does the scene require post processing to be installed?
        public bool RequiresPostProcessing;
        /// does the scene require TextMesh Pro to be installed?
        public bool RequiresTMP;
        /// does the scene require Cinemachine to be installed?
        public bool RequiresCinemachine;

        /// <summary>
        /// On Awake we test for dependencies
        /// </summary>
        protected virtual void Awake()
        {
            TestForDependencies();
        }

        /// <summary>
        /// Checks whether or not dependencies have been correctly installed
        /// </summary>
        protected virtual void TestForDependencies()
        {
            bool missingDependencies = false;
            string missingString = "";
            bool cinemachineFound = false;
            bool tmpFound = false;
            bool postProcessingFound = false;
            
            #if MOREMOUNTAINS_CINEMACHINE_INSTALLED
                cinemachineFound = true;
            #endif
                        
            #if MOREMOUNTAINS_TEXTMESHPRO_INSTALLED
                tmpFound = true;
            #endif
                        
            #if MOREMOUNTAINS_POSTPROCESSING_INSTALLED
                postProcessingFound = true;
            #endif

            if (RequiresCinemachine && !cinemachineFound)
            {
                missingDependencies = true;
                missingString += "Cinemachine";
            }

            if (RequiresTMP && !tmpFound)
            {
                missingDependencies = true;
                if (missingString != "") { missingString += ", "; }
                missingString += "TextMeshPro";
            }
            
            if (RequiresPostProcessing && !postProcessingFound)
            {
                missingDependencies = true;
                if (missingString != "") { missingString += ", "; }
                missingString += "PostProcessing";
            }

            if (missingDependencies)
            {
                Debug.LogError("[DemoPackageTester] It looks like you're missing some dependencies required by this demo ("+missingString+")." +
                               " You'll have to install them to run this demo. You can learn more about how to do so in the documentation, at http://feel-docs.moremountains.com/how-to-install.html" +
                               "\n\n");
            }
        }
    }    
}

