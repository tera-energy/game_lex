using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEngine;

/// <summary>
/// Unity 2020.3 IL2CPP 빌드 보조 스크립트
///
/// [이전 문제] bee.exe가 캐시를 사용해 실제 컴파일을 건너뛰면서
/// Il2cppBuildCache/il2cppOutput이 비어 있고, Temp/에 경로도 없어
/// CopyEmbeddedResourceFiles / CopyMetadataFiles / CopyConfigFiles 등이
/// DirectoryNotFoundException을 던지던 버그.
///
/// [해결] Il2cppBuildCache + il2cpp_android_arm64-v8a 삭제 후 풀 리빌드.
/// 캐시가 없으면 bee.exe가 실제 컴파일을 수행하고 올바른 출력을 생성함.
///
/// [이 스크립트의 역할] 빌드 전 캐시가 비어있는 비정상 상태를 감지해
/// 자동으로 삭제하고 경고를 출력함.
/// </summary>
public class IL2CPPBuildFix : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android) return;

        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string cacheDir = Path.Combine(root, "Library", "Il2cppBuildCache", "Android", "arm64-v8a");
        string il2cppOutputInCache = Path.Combine(cacheDir, "il2cppOutput");

        // 캐시 디렉토리는 있는데 il2cppOutput이 비어있으면 손상된 캐시 → 삭제
        if (Directory.Exists(il2cppOutputInCache))
        {
            bool isEmpty = Directory.GetFiles(il2cppOutputInCache, "*", SearchOption.AllDirectories).Length == 0;
            if (isEmpty)
            {
                Debug.LogWarning("[IL2CPPBuildFix] 손상된 IL2CPP 캐시 감지 (il2cppOutput이 비어있음). 캐시를 삭제하고 풀 리빌드합니다.");
                try
                {
                    string il2cppCacheRoot = Path.Combine(root, "Library", "Il2cppBuildCache");
                    string il2cppAndroidDir = Path.Combine(root, "Library", "il2cpp_android_arm64-v8a");

                    if (Directory.Exists(il2cppCacheRoot))
                        Directory.Delete(il2cppCacheRoot, true);
                    if (Directory.Exists(il2cppAndroidDir))
                        Directory.Delete(il2cppAndroidDir, true);

                    Debug.Log("[IL2CPPBuildFix] 캐시 삭제 완료. 이번 빌드는 풀 컴파일이 수행됩니다 (시간이 더 걸림).");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[IL2CPPBuildFix] 캐시 삭제 실패: " + e.Message);
                }
            }
        }
    }
}
