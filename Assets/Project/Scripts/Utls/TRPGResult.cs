using UnityEngine;

namespace TRPG.Runtime
{
    public readonly struct TRPGResult<T>
    {
        public ResultType Type { get; }

        public T Value { get; }

        public int Code { get; }

        public string Message { get; }

        public bool IsSuccess => Type == ResultType.Success || Type == ResultType.Warn;

        public bool IsFailed => Type == ResultType.Error;

        /// <summary>
        /// 성공
        /// </summary>
        public TRPGResult(T value)
        {
            Value = value;

            Type = ResultType.Success;

            Code = 0;

            Message = string.Empty;
        }

        /// <summary>
        /// 경고
        /// </summary>
        public TRPGResult(T value, WarnCode warnCode)
        {
            Value = value;

            Type = ResultType.Warn;

            Code = (int)warnCode;

            Message = warnCode.ToString();
        }

        /// <summary>
        /// 에러
        /// </summary>
        public TRPGResult(ErrorCode errorCode)
        {
            Value = default;

            Type = ResultType.Error;

            Code = (int)errorCode;

            Message = errorCode.ToString();
        }

        public void Log()
        {
            switch (Type)
            {
                case ResultType.Warn:
                    Debug.LogWarning($"[Warn] {Message}");
                    break;

                case ResultType.Error:
                    Debug.LogError($"[Error] {Message}");
                    break;
            }
        }

        public static TRPGResult<T> Success(T value)
        {
            return new TRPGResult<T>(value);
        }

        public static TRPGResult<T> Warn(T value, WarnCode warnCode)
        {
            return new TRPGResult<T>(value, warnCode);
        }

        public static TRPGResult<T> Error(ErrorCode errorCode)
        {
            return new TRPGResult<T>(errorCode);
        }
    }
}
