using ShareFile.Api.Client.Extensions;
using System;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    internal class PartSizeCalculator
    {
        private FilePartConfig partConfig;
        private int concurrentWorkers;

        public PartSizeCalculator(int concurrentWorkers, FilePartConfig partConfig)
        {
            this.concurrentWorkers = concurrentWorkers;
            this.partConfig = partConfig;
        }

        public long NextPartSize(long lastPartSize, long completedPartSize, TimeSpan elapsedTime)
        {
            //connection speed values are bytes/second
            double estimatedConnectionSpeed = completedPartSize / elapsedTime.TotalSeconds;
            double targetPartSize = estimatedConnectionSpeed * partConfig.TargetPartUploadTime.TotalSeconds;
            double partSizeDelta = targetPartSize - completedPartSize;

            //initial batch of workers will all calculate ~same delta; penalize for >1
            double penalty = concurrentWorkers > 1 ? concurrentWorkers / 2.0 : 1.0;
            partSizeDelta = partSizeDelta / penalty;

            //bound the delta to a multiple of partsize in case of extreme result
            double maxDelta = completedPartSize * (partConfig.MaxPartSizeIncreaseFactor - 1.0);
            double minDelta = completedPartSize * (-1.0 * (partConfig.MaxPartSizeDecreaseFactor - 1.0) / partConfig.MaxPartSizeDecreaseFactor);
            partSizeDelta = partSizeDelta.Bound(maxDelta, minDelta);

            // bound partsize too because we probably got something wrong
            long nextPartSize = lastPartSize;
            nextPartSize += Convert.ToInt64(partSizeDelta);
            nextPartSize = nextPartSize.Bound(partConfig.MaxPartSize, partConfig.MinPartSize);
            return nextPartSize;
        }
    }
}