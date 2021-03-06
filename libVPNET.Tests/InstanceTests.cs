﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VP.Extensions;

namespace VP.Tests
{
    [TestClass]
    public class InstanceTests : InstanceTestBase
    {
        [TestMethod]
        public void Integrity()
        {
            using (var i = new Instance())
            {
                Assert.IsNotNull(i.Avatars);
                Assert.IsNotNull(i.Data);
                Assert.IsNotNull(i.Property);
                Assert.IsNotNull(i.Terrain);
            }
        }

        [TestMethod]
        public void Properties()
        {
            using (var i = new Instance())
            {
                string expectedName = "";
                string expectedWorld = "";
                AvatarPosition expectedPos = AvatarPosition.GroundZero;

                Assert.AreEqual(expectedName, i.Name);
                Assert.AreEqual(expectedWorld, i.World);
                Assert.AreEqual(expectedPos, i.Position);

                expectedName  = Names.Data;
                expectedWorld = Settings.World;
                expectedPos   = Samples.AvPosition;

                i.TestLogin(expectedName).EnterTestWorld();
                i.GoTo(expectedPos);

                Assert.AreEqual(expectedName, i.Name);
                Assert.AreEqual(expectedWorld, i.World);
                Assert.AreEqual(expectedPos, i.Position);
            }
        }

        [TestMethod]
        public void Disposal()
        {
            var i = new Instance();
            i.Dispose();

            VPNetAssert.ThrowsDisposed(_ => i.Dispose());
            VPNetAssert.ThrowsDisposed(_ => i.ConsoleBroadcast("", "", ""));
            VPNetAssert.ThrowsDisposed(_ => i.ConsoleMessage(0, "", "", ""));
            VPNetAssert.ThrowsDisposed(_ => i.Enter(""));
            VPNetAssert.ThrowsDisposed(_ => i.Leave());
            VPNetAssert.ThrowsDisposed(_ => i.GoTo());
            VPNetAssert.ThrowsDisposed(_ => i.Login("", "", ""));
            VPNetAssert.ThrowsDisposed(_ => i.Pump());
            VPNetAssert.ThrowsDisposed(_ => i.Say(""));
        }

        [TestMethod]
        public void Pump()
        {
            NewCmdrData().Pump(1000);
        }

        [TestMethod]
        public void Pump_Async()
        {
            var punch = NewPunch();
            var judy  = NewJudy();
            
            Action pump = () =>
            {
                for (var i = 0; i < 10; i++)
                    TestPump.AllOnce(punch, judy);
            };

            Task.WaitAll(
                Task.Factory.StartNew(() => pump),
                Task.Factory.StartNew(() => pump)
                );
            
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public void Pump_Safety()
        {
            var punch = NewPunch();
            var judy  = NewJudy();

            punch.Chat += (i, c) => punch.Pump();
            judy.Say("MethodPump_Safety");

            TestPump.AllUntilTimeout(punch, judy);
        }

        [TestMethod]
        public void Login_Exceptions()
        {
            using (var cmdrData = new Instance())
            {
                VPNetAssert.ThrowsReasonCode(ReasonCode.ConnectionError,
                    _ => cmdrData.Login(Uniservers.Invalid, "inv", "???", "a")
                    );

                VPNetAssert.ThrowsReasonCode(ReasonCode.Timeout,
                    _ => cmdrData.Login(Uniservers.Timeout, "inv", "???", "a")
                    );

                VPNetAssert.ThrowsReasonCode(ReasonCode.InvalidLogin,
                    _ => cmdrData.Login(Settings.Username, "", "bot")
                    );

                VPNetAssert.ThrowsReasonCode(ReasonCode.StringTooLong,
                    _ => cmdrData.Login(Settings.Username, Settings.Password, Strings.TooLong)
                    );
            }
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public void Login_Safety()
        {
            var punch = NewPunch();
            var judy  = NewJudy();
           
            punch.Chat += (i, c) => punch.Login("", "", "");
            judy.Say("MethodLogin_Safety");

            TestPump.AllUntilTimeout(punch, judy);
        }

        [TestMethod]
        public void Enter_Exceptions()
        {
            using (var cmdrData = new Instance())
            {
                VPNetAssert.ThrowsReasonCode(ReasonCode.NotInUniverse,
                    _ => cmdrData.Enter(Settings.World)
                    );

                cmdrData.TestLogin(Names.Data);

                VPNetAssert.ThrowsReasonCode(ReasonCode.WorldNotFound,
                    _ => cmdrData.Enter(Strings.World404)
                    );

                VPNetAssert.ThrowsReasonCode(ReasonCode.StringTooLong,
                    _ => cmdrData.Enter(Strings.TooLong)
                    );
            }
        }

        [TestMethod]
        public void Enter_Shadow()
        {
            var punch = NewPunch();
            var judy  = NewJudy(false);
            
            punch.Avatars.Enter += (i, a) =>
            {
                if ( a.Name == Names.Judy.AsBotName() )
                    Assert.Fail("Should not have seen {0} enter", Names.Judy);
            };

            judy.Enter(Settings.World, false);

            TestPump.AllUntilTimeout(punch, judy);
        }

        [TestMethod]
        public void Goto()
        {
            var punch = NewPunch();
            var judy  = NewJudy();
            var fired = 0;

            punch.Avatars.Change += (i, a) =>
            {
                if ( a.Name != Names.Judy.AsBotName() )
                    return;

                switch (fired)
                {
                    case 0:
                        Assert.AreEqual(Samples.AvPosition, a.Position);
                        judy.GoTo(AvatarPosition.GroundZero);
                        break;

                    case 1:
                        Assert.AreEqual(AvatarPosition.GroundZero, a.Position);
                        break;
                }

                fired++;
            };

            judy.GoTo(Samples.AvPosition);
            TestPump.AllUntil( () => fired >= 2, punch, judy );
            
            Assert.IsTrue(fired == 2, "State change event not fired exactly twice");
        }

        [TestMethod]
        public void Goto_Exceptions()
        {
            using ( var cmdrData = new Instance().TestLogin(Names.Data) )
                VPNetAssert.ThrowsReasonCode(ReasonCode.NotInWorld, _ => cmdrData.GoTo() );
        }

        [TestMethod]
        public void Say_Unicode()
        {
            var punch = NewPunch();
            var judy  = NewJudy();
            var fired = false;

            punch.Chat += (i, c) =>
            {
                if ( c.Name != Names.Judy.AsBotName() )
                    return;

                Assert.AreEqual(Strings.SampleUnicode, c.Message);
                fired = true;
            };

            judy.Say(Strings.SampleUnicode);
            TestPump.AllUntil( () => fired, punch, judy );

            Assert.IsTrue(fired, "Chat event not fired");
        }

        [TestMethod]
        public void Say_Chunked()
        {
            var punch = NewPunch();
            var judy  = NewJudy();
            var fired = 0;

            punch.Chat += (i, c) =>
            {
                if (c.Name != Names.Judy.AsBotName())
                    return;

                fired++;
            };

            judy.Say(Strings.TooLong);
            TestPump.AllUntil( () => fired >= 3, punch, judy );

            Assert.IsTrue(fired == 3, "Chat event not fired exactly three times");
        }

        [TestMethod]
        public void ConsoleMessage_Chunked()
        {
            var punch = NewPunch();
            var judy  = NewJudy();
            var fired = 0;

            punch.Console += (i, c) =>
            {
                if (c.Name != Names.Judy)
                    return;

                fired++;
            };

            judy.ConsoleMessage(SessionOf(punch), Names.Judy, Strings.TooLong);
            judy.ConsoleMessage(SessionOf(punch) - 1, Names.Judy, Strings.TooLong);
            TestPump.AllUntil( () => fired >= 3, punch, judy );

            Assert.IsTrue(fired == 3, "Console event not fired exactly three times");
        }

        [TestMethod]
        public void ConsoleBroadcast()
        {
            var cmdrData = NewCmdrData();
            var punch    = NewPunch();
            var judy     = NewJudy();
            var fired    = 0;

            cmdrData.Console += (i, c) =>
            {
                if (c.Name != Names.Judy)
                    return;

                Assert.AreEqual(Samples.Color, c.Color);
                Assert.AreEqual(Samples.ChatEffect, c.Effect);
                Assert.AreEqual(Strings.SampleUnicode, c.Message);
                fired++;
            };

            punch.Console += (i, c) =>
            {
                if (c.Name != Names.Judy)
                    return;

                Assert.AreEqual(Samples.Color, c.Color);
                Assert.AreEqual(Samples.ChatEffect, c.Effect);
                Assert.AreEqual(Strings.SampleUnicode, c.Message);
                fired++;
            };

            judy.ConsoleBroadcast(Samples.ChatEffect, Samples.Color, Names.Judy, Strings.SampleUnicode);
            TestPump.AllUntil( () => fired >= 2, cmdrData, punch, judy );

            Assert.IsTrue(fired == 2, "Console event not fired exactly twice");
        }
    }
}