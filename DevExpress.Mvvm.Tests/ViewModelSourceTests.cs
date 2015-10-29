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

namespace DevExpress.Mvvm.Tests.Internal {
    public class POCOViewModel {
        public virtual string Property1 { get; set; }
    }
}
namespace DevExpress.Mvvm.Tests {
    public static class TypeHelper {
        public static PropertyInfo GetProperty(object obj, string propertyName) {
            Type type = obj.GetType();
            PropertyInfo res = null;
            foreach(PropertyInfo info in type.GetProperties())
                if(info.Name == propertyName) {
                    res = info;
                    break;
                }
            return res;
        }
        public static object GetPropertyValue(object obj, string propertyName) {
            Type type = obj.GetType();
            PropertyInfo pInfo = GetProperty(obj, propertyName);
            return pInfo != null ? pInfo.GetValue(obj, null) : null;
        }
        public static AttributeType GetPropertyAttribute<AttributeType>(object obj, string propertyName) where AttributeType : Attribute {
            Type type = obj.GetType();
            PropertyInfo property = GetProperty(obj, propertyName);
            Type attributeType = typeof(AttributeType);

            List<object> result = new List<object>();
            do {
                result.AddRange(property.GetCustomAttributes(true));
                MethodInfo getMethod = property.GetGetMethod();
                if(getMethod == null)
                    break;
                MethodInfo baseMethod = getMethod.GetBaseDefinition();
                if(baseMethod == getMethod)
                    break;
                property = baseMethod.DeclaringType.GetProperty(property.Name);
            } while(property != null);

            foreach(Attribute attribute in result) {
                if(attributeType.IsAssignableFrom(attribute.GetType()))
                    return (AttributeType)attribute;
            }
            return null;
        }
    }


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
    public class POCOViewModel2 {
        public virtual string Property1 { get; set; }
    }
    [TestFixture]
    public class ViewModelSourceTests : BaseWpfFixture {
        #region errors
#pragma warning disable 0618
        #region properties
        public class POCOViewModel_InvalidMetadata_BindableAttributeOnNotVirtualProeprty {
            [BindableProperty]
            public string Property { get; set; }
        }
        [Test]
        public void POCOViewModel_InvalidMetadata_BindableAttributeOnNotVirtualProeprtyTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_InvalidMetadata_BindableAttributeOnNotVirtualProeprty>();
            }, x => Assert.AreEqual("Cannot make non-virtual property bindable: Property.", x.Message));
        }

        public class POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithInternalSetter {
            [BindableProperty]
            public virtual string Property { get; internal set; }
        }
        [Test]
        public void POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithInternalSetterTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithInternalSetter>();
            }, x => Assert.AreEqual("Cannot make property with internal setter bindable: Property.", x.Message));
        }

        public class POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithoutSetter {
            [BindableProperty]
            public virtual string Property { get { return null; } }
        }
        [Test]
        public void POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithoutSetterTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithoutSetter>();
            }, x => Assert.AreEqual("Cannot make property without setter bindable: Property.", x.Message));
        }

        public class POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithoutGetter {
            [BindableProperty]
            public virtual string Property { private get; set; }
        }
        [Test]
        public void POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithoutGetterTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithoutGetter>();
            }, x => Assert.AreEqual("Cannot make property without public getter bindable: Property.", x.Message));
        }

        public class POCOViewModel_SealedClassBase {
            public virtual string Property { get; set; }
        }
        public sealed class POCOViewModel_SealedClass : POCOViewModel_SealedClassBase {
        }
        [Test]
        public void POCOViewModel_SealedClassTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_SealedClass>();
            }, x => Assert.AreEqual("Cannot create dynamic class for the sealed class: POCOViewModel_SealedClass.", x.Message));
        }

        class POCOViewModel_PrivateClass {
        }
        [Test]
        public void POCOViewModel_PrivateClassTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_PrivateClass>();
            }, x => Assert.AreEqual("Cannot create dynamic class for the internal class: POCOViewModel_PrivateClass.", x.Message));
        }

        public class POCOViewModel_TwoPropertyChangedMethods {
            public virtual string Property { get; set; }
            protected void OnPropertyChanged() { }
            protected void OnPropertyChanged(string oldValue) { }
        }
        [Test]
        public void POCOViewModel_TwoPropertyChangedMethodsTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_TwoPropertyChangedMethods>();
            }, x => Assert.AreEqual("More than one property changed method: Property.", x.Message));
        }

        public class POCOViewModel_PrivateChangedMethod {
            public virtual string Property { get; set; }
            void OnPropertyChanged() { }
        }
        [Test]
        public void POCOViewModel_PrivateChangedMethodTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_PrivateChangedMethod>();
            }, x => Assert.AreEqual("Property changed method should be public or protected: OnPropertyChanged.", x.Message));
        }

        public class POCOViewModel_InternalChangedMethod {
            public virtual string Property { get; set; }
            internal void OnPropertyChanged() { }
        }
        [Test]
        public void POCOViewModel_InternalChangedMethodTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_InternalChangedMethod>();
            }, x => Assert.AreEqual("Property changed method should be public or protected: OnPropertyChanged.", x.Message));
        }

        public class POCOViewModel_TwoParametersInChangedMethod {
            public virtual string Property { get; set; }
            protected void OnPropertyChanged(string a, string b) { }
        }
        [Test]
        public void POCOViewModel_TwoParametersInChangedMethodTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_TwoParametersInChangedMethod>();
            }, x => Assert.AreEqual("Property changed method cannot have more than one parameter: OnPropertyChanged.", x.Message));
        }

        public class POCOViewModel_FunctionAsChangedMethod {
            public virtual string Property { get; set; }
            protected int OnPropertyChanged() { return 0; }
        }
        [Test]
        public void POCOViewModel_FunctionAsChangedMethodTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_FunctionAsChangedMethod>();
            }, x => Assert.AreEqual("Property changed method cannot have return type: OnPropertyChanged.", x.Message));
        }

        public class POCOViewModel_InvalidChangedMethodParameterType {
            [BindableProperty(OnPropertyChangedMethodName = "MyOnPropertyChanged")]
            public virtual int Property { get; set; }
            protected void MyOnPropertyChanged(double oldValue) { }
        }
        [Test]
        public void POCOViewModel_InvalidChangedMethodParameterTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_InvalidChangedMethodParameterType>();
            }, x => Assert.AreEqual("Property changed method argument type should match property type: MyOnPropertyChanged.", x.Message));
        }

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
        [MetadataType(typeof(POCOViewModel_InvalidFluentAPIChangedMethodMetadata))]
        public class POCOViewModel_InvalidFluentAPIChangedMethod {
            public class POCOViewModel_InvalidFluentAPIChangedMethodMetadata : IMetadataProvider<POCOViewModel_InvalidFluentAPIChangedMethod> {
                void IMetadataProvider<POCOViewModel_InvalidFluentAPIChangedMethod>.BuildMetadata(MetadataBuilder<POCOViewModel_InvalidFluentAPIChangedMethod> builder) {
                    builder.Property(x => x.Property).OnPropertyChangedCall(x => x.MyOnPropertyChanged());
                }
            }
            public virtual int Property { get; set; }
            void MyOnPropertyChanged() { }
        }
        [Test]
        public void POCOViewModel_InvalidFluentAPIChangedMethodTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_InvalidFluentAPIChangedMethod>();
            }, x => Assert.AreEqual("Property changed method should be public or protected: MyOnPropertyChanged.", x.Message));
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

        [Test]
        public void CallRaiseProeprtyChangedMethodExtensionMethodForNotPOCOViewModelTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                new POCOViewModel().RaisePropertyChanged(x => x.Property1);
            }, x => Assert.AreEqual("Object doesn't implement IPOCOViewModel.", x.Message));
        }

        public class POCOViewModel_INPCImplementor_NoPopertyChanged : INotifyPropertyChanged {
            public POCOViewModel_INPCImplementor_NoPopertyChanged() {
                PropertyChanged(null, null);
            }
            public virtual string Property1 { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
        }
        [Test]
        public void INPCImplementor_NoPopertyChangedTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_INPCImplementor_NoPopertyChanged>();
            }, x => Assert.AreEqual("Class already supports INotifyPropertyChanged, but RaisePropertyChanged(string) method not found: POCOViewModel_INPCImplementor_NoPopertyChanged.", x.Message));
        }

        public class POCOViewModel_INPCImplementor_PrivatePopertyChanged : INotifyPropertyChanged {
            public POCOViewModel_INPCImplementor_PrivatePopertyChanged() {
                PropertyChanged(null, null);
            }
            public virtual string Property1 { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
            void RaisePropertyChanged(string x) { }
        }
        [Test]
        public void INPCImplementor_PrivatePopertyChangedTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_INPCImplementor_PrivatePopertyChanged>();
            }, x => Assert.AreEqual("Class already supports INotifyPropertyChanged, but RaisePropertyChanged(string) method not found: POCOViewModel_INPCImplementor_PrivatePopertyChanged.", x.Message));
        }

        public class POCOViewModel_INPCImplementor_ByRefPopertyChanged : INotifyPropertyChanged {
            public POCOViewModel_INPCImplementor_ByRefPopertyChanged() {
                PropertyChanged(null, null);
            }
            public virtual string Property1 { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
            void RaisePropertyChanged(ref string x) { }
        }
        [Test]
        public void INPCImplementor_ByRefPopertyChangedTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_INPCImplementor_ByRefPopertyChanged>();
            }, x => Assert.AreEqual("Class already supports INotifyPropertyChanged, but RaisePropertyChanged(string) method not found: POCOViewModel_INPCImplementor_ByRefPopertyChanged.", x.Message));
        }

        public class POCOViewModel_INPCImplementor_OutPopertyChanged : INotifyPropertyChanged {
            public POCOViewModel_INPCImplementor_OutPopertyChanged() {
                PropertyChanged(null, null);
            }
            public virtual string Property1 { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
            void RaisePropertyChanged(ref string x) { }
        }
        [Test]
        public void INPCImplementor_OutPopertyChangedTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_INPCImplementor_OutPopertyChanged>();
            }, x => Assert.AreEqual("Class already supports INotifyPropertyChanged, but RaisePropertyChanged(string) method not found: POCOViewModel_INPCImplementor_OutPopertyChanged.", x.Message));
        }

        public class POCOViewModel_INPCImplementor_NoArgPopertyChanged : INotifyPropertyChanged {
            public POCOViewModel_INPCImplementor_NoArgPopertyChanged() {
                PropertyChanged(null, null);
            }
            public virtual string Property1 { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
            void RaisePropertyChanged() { }
        }
        [Test]
        public void INPCImplementor_NoArgPopertyChangedTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_INPCImplementor_NoArgPopertyChanged>();
            }, x => Assert.AreEqual("Class already supports INotifyPropertyChanged, but RaisePropertyChanged(string) method not found: POCOViewModel_INPCImplementor_NoArgPopertyChanged.", x.Message));
        }

        public class POCOViewModel_FinalPropertyBase {
            public virtual int Property { get; set; }
        }
        public class POCOViewModel_FinalProperty : POCOViewModel_FinalPropertyBase {
            [BindableProperty]
            public sealed override int Property { get; set; }
        }
        [Test]
        public void POCOViewModel_FinalPropertyTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<POCOViewModel_FinalProperty>();
            }, x => Assert.AreEqual("Cannot override final property: Property.", x.Message));
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

        public class DesignTimeViewModel {
            public virtual string Property { get; set; }
        }
        [Test]
        public void DesignTimeGeneration() {
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            try {
                DesignTimeViewModel viewModel1 = ViewModelSource.Create<DesignTimeViewModel>();
                Assert.AreNotEqual(typeof(DesignTimeViewModel), viewModel1.GetType());
            } finally {
                ViewModelDesignHelper.IsInDesignModeOverride = null;
            }
        }

        #region IsPOCOViewModel
        #region classes
        public class IsPOCO_Empty { }
        public class IsPOCO_NotVirtualProperty {
            public int MyProperty { get; set; }
        }
        public class IsPOCO_VirtualProperty_INPC : INotifyPropertyChanged {
            public virtual int MyProperty { get; set; }
            event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { } remove { } }
        }

        public class IsPOCO_VirtualProperty {
            public virtual int MyProperty { get; set; }
        }
        public class IsPOCO_VirtualProperty_NoDefaultCtor {
            public virtual int MyProperty { get; set; }
        }
        public class IsPOCO_Method {
            public void Do() { }
        }
        public class IsPOCO_Method_Command {
            public void Do() { }
            public ICommand SomeCommand { get; set; }
        }
        [POCOViewModel]
        public class IsPOCO_Method_Command_Attribute {
            public void Do() { }
            public ICommand SomeCommand { get; set; }
        }
        [POCOViewModel]
        public sealed class IsPOCO_Method_Command_Attribute_Sealed {
            public void Do() { }
            public ICommand SomeCommand { get; set; }
        }
        public class IsPOCO_Method_Command2 {
            public void Do() { }
            public DelegateCommand SomeCommand { get; set; }
        }
        public class IsPOCO_Method_INPC : INotifyPropertyChanged {
            public void Do() { }
            event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { } remove { } }
        }
        public sealed class IsPOCO_Method_Sealed {
            public void Do() { }
        }
        public class IsPOCO_NoDefaultCtor1 {
            protected IsPOCO_NoDefaultCtor1(int x = 13) {
                X = x;
            }
            public virtual int X { get; set; }
        }
        public class IsPOCO_NoDefaultCtor2 {
            protected IsPOCO_NoDefaultCtor2() {
            }
            protected IsPOCO_NoDefaultCtor2(int x = 13) {
                X = x;
            }
            public virtual int X { get; set; }
        }
        public class IsPOCO_NoDefaultCtor3 {
            public IsPOCO_NoDefaultCtor3(object x = null, int y = 0) { }
            public virtual int X { get; set; }
        }
        public class IsPOCO_NoDefaultCtor4 {
            private IsPOCO_NoDefaultCtor4(object x = null, int y = 0) { }
            public virtual int X { get; set; }
        }
        public class IsPOCO_NoDefaultCtor5 {
            private IsPOCO_NoDefaultCtor5(object x = null) { }
            protected IsPOCO_NoDefaultCtor5(object x = null, int y = 0) { }
            public virtual int X { get; set; }
        }
        public class IsPOCO_NoDefaultCtor6 {
            private IsPOCO_NoDefaultCtor6(object x = null) { }
            protected internal IsPOCO_NoDefaultCtor6(object x = null, int y = 0) { }
            public virtual int X { get; set; }
        }

        #endregion
        #endregion

        #region IDataErrorInfo

        public class SimpleDataErrorInfoClass {
            [Required]
            public virtual string StringProp { get; set; }
        }

        [POCOViewModel(ImplementIDataErrorInfo = true)]
        public class AttributedDataErrorInfoClass {
            [Required]
            public virtual string StringProp { get; set; }
        }

        [POCOViewModel(ImplementIDataErrorInfo = true)]
        public class HandwrittenDataErrorInfoClass : IDataErrorInfo {
            [Required]
            public string StringProp { get; set; }
            public string Error { get { return string.Empty; } }
            public string this[string columnName] { get { return IDataErrorInfoHelper.GetErrorText(this, columnName); } }
        }

        [POCOViewModel(ImplementIDataErrorInfo = false)]
        public class HandwrittenDataErrorInfoClass2 : IDataErrorInfo {
            [Required]
            public string StringProp { get; set; }
            public string Error { get { return string.Empty; } }
            public string this[string columnName] { get { return IDataErrorInfoHelper.GetErrorText(this, columnName); } }
        }

        public class HandwrittenDataErrorInfoClass3 : IDataErrorInfo {
            [Required]
            public string StringProp { get; set; }
            public string Error { get { return string.Empty; } }
            public string this[string columnName] { get { return IDataErrorInfoHelper.GetErrorText(this, columnName); } }
        }

        [POCOViewModel(ImplementIDataErrorInfo = true)]
        public class HasErrorPropertyClass {
            public virtual string Error { get; set; }
        }

        [POCOViewModel(ImplementIDataErrorInfo = true)]
        public class HasStringIndexerClass {
            public virtual string this[string i] {
                get { return null; }
            }
        }

        [POCOViewModel(ImplementIDataErrorInfo = true)]
        public class HasIntIndexerClass {
            public virtual string this[int i] {
                get { return null; }
            }
        }

        [Test]
        public void ImplementsDataErrorInfo() {
            Assert.IsFalse(ViewModelSource.Create<SimpleDataErrorInfoClass>() is IDataErrorInfo);

            var vm = ViewModelSource.Create<AttributedDataErrorInfoClass>();
            var asInfo = vm as IDataErrorInfo;

            var hwvm = new HandwrittenDataErrorInfoClass();
            Assert.IsTrue(vm is IDataErrorInfo);
            Assert.AreEqual(hwvm[""], asInfo[""]);
            Assert.AreEqual(hwvm["StringProp"], asInfo["StringProp"]);
            Assert.AreEqual(hwvm.Error, asInfo.Error);
            vm.StringProp = "";
            hwvm.StringProp = "";
            Assert.AreEqual(hwvm["StringProp"], asInfo["StringProp"]);
            Assert.AreEqual(hwvm.Error, asInfo.Error);
            vm.StringProp = "123";
            hwvm.StringProp = "123";
            Assert.AreEqual(hwvm["StringProp"], asInfo["StringProp"]);
            Assert.AreEqual(hwvm.Error, asInfo.Error);
        }

        [Test]
        public void DoesntThowOnConflicts() {
            ViewModelSource.Create<HasErrorPropertyClass>();
            ViewModelSource.Create<HasStringIndexerClass>();
            ViewModelSource.Create<HasIntIndexerClass>();
        }

        [Test]
        public void ThrowsIfAlreadyImplemented() {
            try {
                ViewModelSource.Create<HandwrittenDataErrorInfoClass>();
                Assert.Fail();
            } catch(ViewModelSourceException) { } catch {
                Assert.Fail();
            }
            ViewModelSource.Create<HandwrittenDataErrorInfoClass2>();
            ViewModelSource.Create<HandwrittenDataErrorInfoClass3>();
        }

        #endregion

        void CheckBindableProperty<T, TProperty>(T viewModel, Expression<Func<T, TProperty>> propertyExpression, Action<T, TProperty> setValueAction, TProperty value1, TProperty value2, Action<T, TProperty> checkOnPropertyChangedResult = null) {
            CheckBindablePropertyCore(viewModel, propertyExpression, setValueAction, value1, value2, true, checkOnPropertyChangedResult);
        }
        void CheckNotBindableProperty<T, TProperty>(T viewModel, Expression<Func<T, TProperty>> propertyExpression, Action<T, TProperty> setValueAction, TProperty value1, TProperty value2) {
            CheckBindablePropertyCore(viewModel, propertyExpression, setValueAction, value1, value2, false, null);
        }
        void CheckBindablePropertyCore<T, TProperty>(T viewModel, Expression<Func<T, TProperty>> propertyExpression, Action<T, TProperty> setValueAction, TProperty value1, TProperty value2, bool bindable, Action<T, TProperty> checkOnPropertyChangedResult) {
            Assert.AreNotEqual(value1, value2);
            Func<T, TProperty> getValue = propertyExpression.Compile();

            int propertyChangedFireCount = 0;
            PropertyChangedEventHandler handler = (o, e) => {
                Assert.AreEqual(viewModel, o);
                Assert.AreEqual(BindableBase.GetPropertyNameFast(propertyExpression), e.PropertyName);
                propertyChangedFireCount++;
            };
            ((INotifyPropertyChanged)viewModel).PropertyChanged += handler;
            Assert.AreEqual(0, propertyChangedFireCount);
            TProperty oldValue = getValue(viewModel);
            setValueAction(viewModel, value1);
            checkOnPropertyChangedResult.Do(x => x(viewModel, oldValue));
            if(bindable) {
                Assert.AreEqual(value1, getValue(viewModel));
                Assert.AreEqual(1, propertyChangedFireCount);
            } else {
                Assert.AreEqual(0, propertyChangedFireCount);
            }
            ((INotifyPropertyChanged)viewModel).PropertyChanged -= handler;
            setValueAction(viewModel, value2);
            setValueAction(viewModel, value2);
            checkOnPropertyChangedResult.Do(x => x(viewModel, value1));
            if(bindable) {
                Assert.AreEqual(value2, getValue(viewModel));
                Assert.AreEqual(1, propertyChangedFireCount);
            } else {
                Assert.AreEqual(0, propertyChangedFireCount);
            }
        }
        ICommand CheckCommand<T>(T viewModel, Expression<Action<T>> methodExpression, Action<T> checkExecuteResult, bool isAsyncCommand = false) {
            string commandName = GetCommandName<T>(methodExpression);
            ICommand command = (ICommand)TypeHelper.GetPropertyValue(viewModel, commandName);
            Assert.IsNotNull(command);
            Assert.AreSame(command, TypeHelper.GetPropertyValue(viewModel, commandName));
            Assert.IsTrue(command.CanExecute(null));
            command.Execute(null);
            if(isAsyncCommand)
                Thread.Sleep(400);
            checkExecuteResult(viewModel);
            return command;
        }
        void CheckNoCommand<T>(T viewModel, string methodName) {
            Assert.IsNotNull(typeof(T).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
            string commandName = methodName + "Command";
            Assert.IsNull(TypeHelper.GetProperty(viewModel, commandName));
        }
        static string GetCommandName<T>(Expression<Action<T>> methodExpression) {
            return ExpressionHelper.GetMethod(methodExpression).Name + "Command";
        }
    }
}