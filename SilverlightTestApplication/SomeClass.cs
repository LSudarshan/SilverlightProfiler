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

        private void D(string text)
        {
        }

        private void B()
        {
            
        }
    }
}