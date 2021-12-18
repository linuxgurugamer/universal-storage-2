
using UnityEngine;

namespace UniversalStorage2
{
    public class USdebugMessages : MonoBehaviour
    {
        public bool debugMode = true;
        public string moduleName = string.Empty;

        public enum OutputMode
        {
            screen,
            log,
            both,
            none,
        }

        OutputMode outputMode = OutputMode.log;
        public float postToScreenDuration = 5f;

        public USdebugMessages()
        {
        }

        public USdebugMessages(bool _debugMode, string _moduleName)
        {
            debugMode = _debugMode;
            outputMode = OutputMode.log;
            moduleName = _moduleName + ": ";
        }

        public USdebugMessages(bool _debugMode, OutputMode _outputMode, float _postToScreenDuration)
        {
            debugMode = _debugMode;
            outputMode = _outputMode;
            postToScreenDuration = _postToScreenDuration;
        }

        public void Log(string input)
        {
            debugMessage(input);
        }

        public void debugMessage(object input) // fully automatic mode, posts to screen or or log depending on general setting
        {
            if (debugMode)
            {
                switch (outputMode)
                {
                    case OutputMode.both:
                        debugMessage(input, true, postToScreenDuration);
                        break;
                    case OutputMode.log:
                        debugMessage(input, true, 0f);
                        break;
                    case OutputMode.screen:
                        debugMessage(input, false, postToScreenDuration);
                        break;
                }
            }
        }

        public void debugMessage(object input, bool postToLog, float postToScreenDuration) // fully manual mode, post to screen or log depending on parameters
        {
            if (debugMode)
            {
                PostMessage(input, postToLog, postToScreenDuration);
            }
        }

        public void debugMessage(object input, float postToScreenDuration) // semi-manual mode: posts to screen regardless of general setting, and to log, depending on general setting
        {
            if (debugMode)
            {
                switch (outputMode)
                {
                    case OutputMode.both:
                        debugMessage(input, true, postToScreenDuration);
                        break;
                    case OutputMode.log:
                        debugMessage(input, true, postToScreenDuration);
                        break;
                    case OutputMode.screen:
                        debugMessage(input, false, postToScreenDuration);
                        break;
                }
            }
        }

        public void PostMessage(object input, bool postToLog, float postToScreenDuration) // Posts uninstantiated, so it doesn't care about debugMode.
        {
            if (postToLog)
            {
                Debug.Log(moduleName + input);
            }
            if (postToScreenDuration > 0f) // will only work in the flight scene, gives an error in other places.
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(input.ToString(), postToScreenDuration, ScreenMessageStyle.UPPER_RIGHT));
                //nextPostDuration = postToScreenDuration;
            }
        }

        public static void USStaticLog(string log, params object[] stringObjects)
        {
            log = string.Format(log, stringObjects);
            string finalLog = string.Format("[Universal_Storage] {0}", log);
            Debug.Log(finalLog);
        }
    }
}