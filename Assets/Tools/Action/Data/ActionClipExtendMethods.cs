using UnityEngine;

namespace Tools.Action.Data
{
    public static class ActionClipExtendMethods
    {
        public static ActionCollideDetectionClipData Clone(this ActionCollideDetectionClipData clip,ActionCollideDetectionType type)
        {
            var newClip = new ActionCollideDetectionClipData();
            newClip.type = type;
            newClip.startTime = clip.startTime;
            newClip.duration = clip.duration;
            newClip.startTick = clip.startTick;
            newClip.durationTicks = clip.durationTicks;
            newClip.colliderLayerName = clip.colliderLayerName;
            newClip.sliceId = clip.sliceId;
            ActionCollideDetectionBaseData cloneData = clip.data.Clone(type);
            cloneData = clip.data;
            newClip.data = cloneData;
            return newClip;
        }
        public static ActionCollideDetectionBaseData Clone(this ActionCollideDetectionBaseData data,ActionCollideDetectionType type)
        {
            ActionCollideDetectionBaseData cloneData;
            switch (data)
            {
                case ActionCollideDetectionBoxData boxData:
                    cloneData = new ActionCollideDetectionBoxData()
                    {
                        localRotation =  boxData.localRotation,
                        localPosition =  boxData.localPosition,
                        size =  boxData.size,
                    };
                    break;
                case ActionCollideDetectionSphereData sphereData:
                    cloneData = new ActionCollideDetectionSphereData()
                    {
                        localPosition =   sphereData.localPosition,
                        radius = sphereData.radius,
                    };
                    break;
                case ActionCollideDetectionSectorData sectorData:
                    cloneData = new ActionCollideDetectionSectorData()
                    {
                        localPosition = sectorData.localPosition,
                        localRotation = sectorData.localRotation,
                        insideRadius = sectorData.insideRadius,
                        outsideRadius = sectorData.outsideRadius,
                        height = sectorData.height,
                        angle = sectorData.angle,
                        anglePivot = sectorData.anglePivot,
                    };
                    break;
                default:
                    Debug.LogError("Unhandled action collide detection type: " + type);
                    break;
            }
            cloneData = data;
            return cloneData;
        }
    }
}