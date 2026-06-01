using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 기존 PF_Map 프리팹의 Missing Script를 막기 위한 호환용 컴포넌트입니다.
    /// 런타임 맵 제어 로직은 WorldManager partial 파일들로 통합되었습니다.
    /// </summary>
    [Obsolete("Map runtime logic is integrated into WorldManager.")]
    public class MapController : MonoBehaviour
    {

    }
}
