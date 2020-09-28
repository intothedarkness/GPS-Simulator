using System.Collections.Generic;

namespace SharpGpx.GPX1_1
{
    public static class Extensions
    {
        
        public static List<T> AddItem<T>(this List<T> collection, T wptType)
        {
            collection.Add(wptType);
            return collection;
        }

        public static wptTypeCollection Addwpt(this wptTypeCollection collection, wptType wptType)
        {
            collection.Add(wptType);
            return collection;
        }

        public static rteTypeCollection Addrte(this rteTypeCollection collection, rteType rteType)
        {
            collection.Add(rteType);
            return collection;
        }

        public static trkTypeCollection Addtrk(this trkTypeCollection collection, trkType trkType)
        {
            collection.Add(trkType);
            return collection;
        }

        public static linkTypeCollection AddLink(this linkTypeCollection collection, GPX1_1.linkType link)
        {
            collection.Add(link);
            return collection;
        }

        public static trksegTypeCollection Addtrkseg(this trksegTypeCollection collection, trksegType trksegType)
        {
            collection.Add(trksegType);
            return collection;
        }

        public static ptTypeCollection Addpt(this ptTypeCollection collection, ptType ptType)
        {
            collection.Add(ptType);
            return collection;
        }
    }
}
