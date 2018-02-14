using System;
using System.Collections.Generic;
using Android.Runtime;
using Android.Support.V4.App;

namespace AppOnkyo.ADAPTER
{
    public class ViewPagerAdapter : FragmentPagerAdapter
    {
        private readonly List<Fragment> liFragments = new List<Fragment>();

        public ViewPagerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public ViewPagerAdapter(FragmentManager fm) : base(fm)
        {
        }

        public override int Count => liFragments.Count;

        public override Fragment GetItem(int p)
        {
            return liFragments[p];
        }

        public void AddFragment(Fragment f)
        {
            liFragments.Add(f);
        }
    }
}