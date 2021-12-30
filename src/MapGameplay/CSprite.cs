using UnhedderEngine;

namespace MapGameplay
{
    public class CSprite : CRendering
    {
        public Texture[] Textures;
        public int ForwardIndex = 0;
        public int RightIndex = 0;
        public int BackwardIndex = 0;
        public int LeftIndex = 0;
        public int[] AnimationOffsets;
        public float AnimationFps = 1;
        public float AnimationState;
        public bool OnlyAnimateWhenMoving;
    }
}
