using UnityEngine;
using System;

namespace TRPG.Runtime
{
    /// <summary>
    /// Unity 기본 로그 핸들러 앞에 시간과 로그 레벨을 붙이는 로그 핸들러입니다.
    /// </summary>
    public class CustomLogHandler : ILogHandler
    {
        private readonly ILogHandler m_DefaultHandler = Debug.unityLogger.logHandler;

        /// <summary>
        /// Unity 로그 메시지에 시간과 로그 레벨을 붙여 기본 핸들러로 전달합니다.
        /// </summary>
        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            // 1. 시간 및 로그 레벨 설정
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string level = logType.ToString().ToUpper();

            // 2. 메시지 조립
            string message = string.Format(format, args);

            // 3. spdlog 스타일 패턴 적용: [%Y-%m-%d %H:%M:%S.%e] [%l] %v
            // (참고: C# 환경에서 호출 파일/라인은 StackTrace 사용 시 성능 비용이 발생함)
            string finalMessage = $"[{timestamp}] [{level}] {message}";

            m_DefaultHandler.LogFormat(logType, context, "{0}", finalMessage);
        }

        /// <summary>
        /// 예외 로그는 Unity 기본 핸들러에 위임합니다.
        /// </summary>
        public void LogException(Exception exception, UnityEngine.Object context)
        {
            m_DefaultHandler.LogException(exception, context);
        }
    }

    /// <summary>
    /// 애플리케이션 시작 시 커스텀 로그 핸들러를 등록합니다.
    /// </summary>
    public static class LogInitializer
    {
        /// <summary>
        /// 씬 로드 전에 커스텀 로그 핸들러를 Unity 로거에 연결합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Debug.unityLogger.logHandler = new CustomLogHandler();
        }
    }
}
