using System;

namespace SilverlightTestApplication
{
    public class SomeClass
    {
        public void A()
        {
            B();
        }

        public void C()
        {
            D("text");
        }


        public static void ThrowsException()
        {
            int a = 0;
            throw new Exception("blah");
        }

        private void D(string text)
        {
        }

        private void B()
        {
            
        }

        public static void E()
        {
            try
            {
                ThrowsException();
            }catch(Exception e)
            {
                if("abc".Contains("efg"))
                {
                    throw;
                }
            }
        }
    }
}