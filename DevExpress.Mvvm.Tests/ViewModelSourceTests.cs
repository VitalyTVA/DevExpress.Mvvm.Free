#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Silverlight.Testing;
#else
using NUnit.Framework;
using System.Windows.Threading;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using System.Windows.Controls;
using System.Windows.Data;
using Moq;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.TestClasses.VB;
using System.Threading.Tasks;
using System.Threading;
using Expression = System.Linq.Expressions.Expression;

namespace DevExpress.Mvvm.Tests {


    [CLSCompliant(false)]
    public class POCOViewModel {
        internal string NotPublicProperty { get; set; }
        public string NotVirtualProperty { get; set; }
        public virtual string ProtectedGetterProperty { protected internal get; set; }
        public virtual string InternalSetterProperty { get; internal set; }
        string notAutoImplementedProperty;
        public virtual string NotAutoImplementedProperty { get { return notAutoImplementedProperty; } set { notAutoImplementedProperty = value; } }

        public virtual string Property1 { get; set; }
        public virtual string Property2 { get; set; }
        public virtual object Property3 { get; set; }
        public virtual int Property4 { get; set; }
        public virtual Point Property5 { get; set; }
        public virtual int? Property6 { get; set; }
        public virtual string ProtectedSetterProperty { get; protected internal set; }
    }
    [TestFixture]
    public class ViewModelSourceTests : BaseWpfFixture {
        #region errors
#pragma warning disable 0618
        #region properties

        public class POCOViewModel_InvalidChangedMethodName {
            [BindableProperty(OnPropertyChangedMethodName = "MyOnPropertyChanged")]
            public virtual int Property { get; set; }
            protected void OnPropertyChanged(double oldValue) { }
        }
        [Test]
        public void POCOViewModel_InvalidChangedMethodNameTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_InvalidChangedMethodName>();
            }, x => Assert.AreEqual("Property changed method not found: MyOnPropertyChanged.", x.Message));
        }

        public class InvalidIPOCOViewModelImplementation : IPOCOViewModel {
            void IPOCOViewModel.RaisePropertyChanged(string propertyName) {
                throw new NotImplementedException();
            }
        }
        [Test]
        public void InvalidIPOCOViewModelImplementationTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<InvalidIPOCOViewModelImplementation>();
            }, x => Assert.AreEqual("Type cannot implement IPOCOViewModel: InvalidIPOCOViewModelImplementation.", x.Message));
        }
        #endregion

        #region commands
        public class POCOViewModel_MemberWithCommandName {
            public void Show() { ShowCommand++; }
            int ShowCommand = 0;
        }
        [Test]
        public void POCOViewModel_MemberWithCommandNameTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_MemberWithCommandName>();
            }, x => Assert.AreEqual("Member with the same command name already exists: ShowCommand.", x.Message));
        }
        public class POCOViewModel_MemberWithCommandName2 {
            public void Show() { ShowCommand(null, null); }
            public static event EventHandler ShowCommand;
        }
        [Test]
        public void POCOViewModel_MemberWithCommandNameTest2() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_MemberWithCommandName2>();
            }, x => Assert.AreEqual("Member with the same command name already exists: ShowCommand.", x.Message));
        }

        public class DuplicateNamesViewModel {
            [Command(Name = "MyCommand")]
            public void Method1() { }
            [Command(Name = "MyCommand")]
            public void Method2() { }
        }
        [Test]
        public void CommandAttribute_DuplicateNamesTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<DuplicateNamesViewModel>();
            }, x => Assert.AreEqual("Member with the same command name already exists: MyCommand.", x.Message));
        }

        public class NotPublicMethodViewModel {
            [Command]
            void NotPublicMethod() { }
        }
        [Test]
        public void CommandAttribute_NotPublicMethodTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<NotPublicMethodViewModel>();
            }, x => Assert.AreEqual("Method should be public: NotPublicMethod.", x.Message));
        }

        public class TooMuchArgumentsMethodViewModel {
            [Command]
            public void TooMuchArgumentsMethod(int a, int b) { }
        }
        [Test]
        public void CommandAttribute_TooMuchArgumentsMethodTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<TooMuchArgumentsMethodViewModel>();
            }, x => Assert.AreEqual("Method cannot have more than one parameter: TooMuchArgumentsMethod.", x.Message));
        }

        public class OutParameterMethodViewModel {
            [Command]
            public void OutParameterMethod(out int a) { a = 0; }
        }
        [Test]
        public void CommandAttribute_OutParameterMethodTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<OutParameterMethodViewModel>();
            }, x => Assert.AreEqual("Method cannot have out or reference parameter: OutParameterMethod.", x.Message));
        }

        public class RefParameterMethodViewModel {
            [Command]
            public void RefParameterMethod(ref int a) { a = 0; }
        }
        [Test]
        public void CommandAttribute_RefParameterMethodTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<RefParameterMethodViewModel>();
            }, x => Assert.AreEqual("Method cannot have out or reference parameter: RefParameterMethod.", x.Message));
        }

        public class CanExecuteParameterCountMismatchViewModel {
            [Command]
            public void Method() { }
            public bool CanMethod(int a) { return true; }
        }
        [Test]
        public void CommandAttribute_CanExecuteParameterCountMismatchTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<CanExecuteParameterCountMismatchViewModel>();
            }, x => Assert.AreEqual("Can execute method has incorrect parameters: CanMethod.", x.Message));
        }

        public class CanExecuteParametersMismatchViewModel {
            [Command]
            public void Method(long a) { }
            public bool CanMethod(int a) { return true; }
        }
        [Test]
        public void CommandAttribute_CanExecuteParametersMismatchTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<CanExecuteParametersMismatchViewModel>();
            }, x => Assert.AreEqual("Can execute method has incorrect parameters: CanMethod.", x.Message));
        }

        public class CanExecuteParametersMismatchViewModel2 {
            [Command]
            public void Method(int a) { }
            public bool CanMethod(out int a) { a = 0; return true; }
        }
        [Test]
        public void CommandAttribute_CanExecuteParametersMismatchTest2() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<CanExecuteParametersMismatchViewModel2>();
            }, x => Assert.AreEqual("Can execute method has incorrect parameters: CanMethod.", x.Message));
        }

        public class NotPublicCanExecuteViewModel {
            [Command]
            public void Method() { }
            bool CanMethod() { return true; }
        }
        [Test]
        public void CommandAttribute_NotPublicCanExecuteTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<NotPublicCanExecuteViewModel>();
            }, x => Assert.AreEqual("Method should be public: CanMethod.", x.Message));
        }

        public class InvalidCanExecuteMethodNameViewModel {
            [Command(CanExecuteMethodName = "CanMethod_")]
            public void Method() { }
        }
        [Test]
        public void CommandAttribute_InvalidCanExecuteMethodNameTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<InvalidCanExecuteMethodNameViewModel>();
            }, x => Assert.AreEqual("Method not found: CanMethod_.", x.Message));
        }
        #endregion

        #region ctors
        public class InternalCtor {
            public InternalCtor() { }
            internal InternalCtor(int x) { }
        }
        [Test]
        public void InternalCtorTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create(() => new InternalCtor(0));
            }, x => Assert.AreEqual("Constructor not found.", x.Message));
        }

        public class OnlyInternalCtor {
            internal OnlyInternalCtor(int x) { }
        }
        [Test]
        public void OnlyInternalCtorTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create(() => new OnlyInternalCtor(0));
            }, x => Assert.AreEqual("Type has no accessible constructors: OnlyInternalCtor.", x.Message));
        }

        public class POCOViewModel_CreateViaGenericParameters {
            string constructorInfo;
            public string ConstructorInfo { get { return constructorInfo; } set { Assert.IsTrue(this is IPOCOViewModel); constructorInfo = value; } }
            public POCOViewModel_CreateViaGenericParameters() {
                ConstructorInfo = "";
            }
            public POCOViewModel_CreateViaGenericParameters(string p1) {
                ConstructorInfo = p1;
            }
            public POCOViewModel_CreateViaGenericParameters(string p1, int p2) {
                ConstructorInfo = p1 + p2;
            }
            public POCOViewModel_CreateViaGenericParameters(string p1, int p2, bool p3) {
                ConstructorInfo = p1 + p2 + p3;
            }
            public POCOViewModel_CreateViaGenericParameters(string p1, int p2, bool p3, double p4) {
                ConstructorInfo = p1 + p2 + p3 + p4;
            }
            public POCOViewModel_CreateViaGenericParameters(string p1, int p2, bool p3, double p4, char p5) {
                ConstructorInfo = p1 + p2 + p3 + p4 + p5;
            }
            public POCOViewModel_CreateViaGenericParameters(string p1, int p2, bool p3, double p4, char p5, Visibility p6) {
                ConstructorInfo = p1 + p2 + p3 + p4 + p5 + p6;
            }
            public POCOViewModel_CreateViaGenericParameters(string p1, int p2, bool p3, double p4, char p5, Visibility p6, decimal p7) {
                ConstructorInfo = p1 + p2 + p3 + p4 + p5 + p6 + p7;
            }
            public POCOViewModel_CreateViaGenericParameters(string p1, int p2, bool p3, double p4, char p5, Visibility p6, decimal p7, float p8) {
                ConstructorInfo = p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8;
            }
            public POCOViewModel_CreateViaGenericParameters(string p1, int p2, bool p3, double p4, char p5, Visibility p6, decimal p7, float p8, long p9) {
                ConstructorInfo = p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;
            }
            public POCOViewModel_CreateViaGenericParameters(string p1, int p2, bool p3, double p4, char p5, Visibility p6, decimal p7, float p8, long p9, short p10) {
                ConstructorInfo = p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10;
            }
        }

        [Test]
        public void CreateViaGenericParameters_InvalidParaneterTypes() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                var viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create(1);
            }, x => Assert.AreEqual("Constructor not found.", x.Message));
        }

        [Test]
        public void GetFactory_MemberInitializers() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Factory((string p1) => new POCOViewModel_CreateViaGenericParameters(p1));
                ViewModelSource.Factory((string p1) => new POCOViewModel_CreateViaGenericParameters(p1) { ConstructorInfo = "" });
            }, x => Assert.AreEqual("Constructor expression can only be of NewExpression type.", x.Message));
        }

        [Test]
        public void GetFactory_InvalidCtorParameters() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Factory((string p1) => new POCOViewModel_CreateViaGenericParameters(p1));
                ViewModelSource.Factory((string p1) => new POCOViewModel_CreateViaGenericParameters("x"));
            }, x => Assert.AreEqual("Constructor expression can refer only to its arguments.", x.Message));
        }
        #endregion

#pragma warning restore 0618
        #endregion
    }
}