using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LanguageTrainer.Source.Tools
{
    public class TextAnimation
    {
        public PathGeometry From { get; set; }
        public PathGeometry To { get; set; }
        public double Speed { get; }
        public EasingFunctionBase EaseFuntion { get; }

        private int size;
        private int step;
        public double Progress => step / size;

        private bool isCompleted;
        public bool IsCompleted => isCompleted;
        public bool IsCached { get; }
        public List<PathGeometry> Cache { get; }

        public bool Move()
        {
            if (step > size) return false;

            step++;

            return true;
        }

        public PathGeometry GetFrame()
        {
            if (isCompleted) return Cache[step];


            PathGeometry frame;
            
            if (step > size)
            {
                isCompleted = true;
                frame = To;
            }
            else
            {
                var easeP = EaseFuntion.Ease(Progress);
                frame = From.Clone();

                Morph.To(frame, To, easeP);
            }

            if (IsCached) Cache.Add(frame);

            return frame;
        }

        public void Reload()
        {
            step = 0;
        }

        public TextAnimation(
            PathGeometry from,
            PathGeometry to,
            EasingFunctionBase easingFunction,
            double speed = 0.05,
            bool isCached = false
        )
        {
            step = 0;

            From = from;
            To = to;
            Speed = speed;
            IsCached = isCached;
            EaseFuntion = easingFunction;

            size = (int)(1 / Speed) - 2;
            
            if (isCached)
            {
                Cache = new List<PathGeometry>(size + 2) { from };
            }
        }

        public TextAnimation(
            PathGeometry from,
            PathGeometry to,
            double speed,
            bool isCached
        ) : this(from, to, new PowerEase(), speed, isCached)
        {

        }

        public TextAnimation(List<PathGeometry> anim)
        {
            step = 0;
            isCompleted = true;

            Cache = anim;
            IsCached = true;

            size = anim.Count - 2;
            Speed = 1 / (double)size;

            From = anim.First();
            To = anim.Last();
        }
    }
}
