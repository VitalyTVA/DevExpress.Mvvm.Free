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

        [Test]
        public void CallRaiseCommandChangedMethodExtensionMethodForNotPOCOViewModelTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                new POCOCommandsViewModel().RaiseCanExecuteChanged(x => x.Save());
            }, x => Assert.AreEqual("Object doesn't implement IPOCOViewModel.", x.Message));
        }
        [Test]
        public void CallGetCommandMethodExtensionMethodForNotPOCOViewModelTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                new POCOCommandsViewModel().GetCommand(x => x.Save());
            }, x => Assert.AreEqual("Object doesn't implement IPOCOViewModel.", x.Message));
        }
        [Test]
        public void CallRaiseCommandChangedMethodExtensionMethodForNotCommandMethod() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create<CommandAttributeViewModel>().RaiseCanExecuteChanged(x => x.NoAttribute());
            }, x => Assert.AreEqual("Command not found: NoAttributeCommand.", x.Message));
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

        #region services
        public class Services_NotServiceType {
            [ServiceProperty]
            public virtual SomeService Property { get { return null; } }
        }
        [Test]
        public void Services_NotServiceTypeTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create(() => new Services_NotServiceType());
            }, x => Assert.AreEqual("Service properties should have an interface type: Property.", x.Message));
        }

        public class Services_PropertyIsNotVirtual {
            [ServiceProperty]
            public IMessageBoxService Property { get { return null; } }
        }
        [Test]
        public void Services_PropertyIsNotVirtualTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create(() => new Services_PropertyIsNotVirtual());
            }, x => Assert.AreEqual("Property is not virtual: Property.", x.Message));
        }

        public class Services_PropertyIsSealedBase {
            public virtual IMessageBoxService Property { get { return null; } }
        }
        public class Services_PropertyIsSealed : Services_PropertyIsSealedBase {
            [ServiceProperty]
            public override sealed IMessageBoxService Property { get { return null; } }
        }
        [Test]
        public void Services_PropertyIsSealedTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create(() => new Services_PropertyIsSealed());
            }, x => Assert.AreEqual("Cannot override final property: Property.", x.Message));
        }

        public class Services_PropertyHasSetter {
            [ServiceProperty]
            public virtual IMessageBoxService Property { get; set; }
        }
        [Test]
        public void Services_PropertyHasSetterTest() {
            AssertHelper.AssertThrows<ViewModelSourceException>(() => {
                ViewModelSource.Create(() => new Services_PropertyHasSetter());
            }, x => Assert.AreEqual("Property with setter cannot be Service Property: Property.", x.Message));
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
        [Test]
        public void IsInDesignMode_NotPOCOViewModelTest() {
            POCOViewModel_PropertyChanged viewModel = new POCOViewModel_PropertyChanged();
            Assert.IsFalse(viewModel.IsInDesignMode());
        }

        #region property changed
        public class POCOViewModel_PropertyChangedBase {
            protected virtual void OnProtectedChangedMethodWithParamChanged(string oldValue) { }
            public virtual bool SealedProperty { get; set; }
        }
        public class POCOViewModel_PropertyChanged : POCOViewModel_PropertyChangedBase {
            public string ProtectedChangedMethodWithParamOldValue;
            public bool OnProtectedChangedMethodWithParamChangedCalled;
            public virtual string ProtectedChangedMethodWithParam { get; set; }
            protected override void OnProtectedChangedMethodWithParamChanged(string oldValue) {
                Assert.AreNotEqual(ProtectedChangedMethodWithParam, ProtectedChangedMethodWithParamOldValue);
                OnProtectedChangedMethodWithParamChangedCalled = true;
                ProtectedChangedMethodWithParamOldValue = oldValue;
            }
            public sealed override bool SealedProperty { get; set; }

            public int PublicChangedMethodWithoutParamOldValue;
            public virtual int PublicChangedMethodWithoutParam { get; set; }
            public void OnPublicChangedMethodWithoutParamChanged() {
                PublicChangedMethodWithoutParamOldValue++;
            }

            public int ProtectedInternalChangedMethodWithoutParamOldValue;
            public virtual int ProtectedInternalChangedMethodWithoutParam { get; set; }
            protected internal void OnProtectedInternalChangedMethodWithoutParamChanged() {
                ProtectedInternalChangedMethodWithoutParamOldValue++;
            }
        }
        public class ViewModelWithFunctionCommandMethod {
            [Command]
            public bool Save() {
                return true;
            }
        }
        [Test]
        public void GetCommandFromFunction_VB() {
            ViewModelWithFunctionCommandMethod viewModel = ViewModelSource.Create<ViewModelWithFunctionCommandMethod>();

            ParameterExpression parameter1 = Expression.Parameter(typeof(ViewModelWithFunctionCommandMethod), "x");
            MethodCallExpression methodCallExpression = Expression.Call((Expression)parameter1, typeof(ViewModelWithFunctionCommandMethod).GetMethod("Save", BindingFlags.Public | BindingFlags.Instance), new Expression[0]);
            var expression = Expression.Lambda<Func<ViewModelWithFunctionCommandMethod, bool>>(methodCallExpression, parameter1);

            ParameterExpression parameter2 = Expression.Parameter(typeof(ViewModelWithFunctionCommandMethod), "a0");
            var methodExpression = Expression.Lambda<Action<ViewModelWithFunctionCommandMethod>>(Expression.Invoke(expression, parameter2), parameter2);
            Assert.IsNotNull(POCOViewModelExtensions.GetCommand(viewModel, methodExpression));
        }
        #endregion

        #region metadata
        public class POCOViewModel_WithMetadata {
            [BindableProperty(false)]
            public virtual string NotBindableProperty { get; set; }

            string notAutoImplementedProperty;
            [BindableProperty]
            public virtual string NotAutoImplementedProperty { get { return notAutoImplementedProperty; } set { notAutoImplementedProperty = value; } }

            public string CustomProperytChangedOldValue;
            string customProperytChanged;
            [BindableProperty(OnPropertyChangedMethodName = "OnCustomProperytChanged")]
            public virtual string CustomProperytChanged { get { return customProperytChanged; } set { customProperytChanged = value; } }
            protected void OnCustomProperytChanged(string oldValue) {
                CustomProperytChangedOldValue = oldValue;
            }

            [BindableProperty(OnPropertyChangingMethodName = "MyPropertyChanging")]
            public virtual string PropertyChanging { get; set; }
            public string PropertyChangingNewValue;
            protected void MyPropertyChanging(string newValue) {
                Assert.AreNotEqual(newValue, PropertyChanging);
                PropertyChangingNewValue = newValue;
            }
        }
        [MetadataType(typeof(POCOViewModel_WithMetadata_FluentAPIMetadata))]
        public class POCOViewModel_WithMetadata_FluentAPI {
            public class POCOViewModel_WithMetadata_FluentAPIMetadata : IMetadataProvider<POCOViewModel_WithMetadata_FluentAPI> {
                void IMetadataProvider<POCOViewModel_WithMetadata_FluentAPI>.BuildMetadata(MetadataBuilder<POCOViewModel_WithMetadata_FluentAPI> builder) {
                    builder.Property(x => x.NotBindableProperty).DoNotMakeBindable();
                    builder.Property(x => x.NotAutoImplementedProperty).MakeBindable();
                    builder.Property(x => x.CustomProperytChanged).OnPropertyChangedCall(x => x.OnCustomProperytChanged(null));
                    builder.Property(x => x.PropertyChanging).OnPropertyChangingCall(x => x.MyPropertyChanging(null));
                }
            }
            public virtual string NotBindableProperty { get; set; }

            string notAutoImplementedProperty;
            public virtual string NotAutoImplementedProperty { get { return notAutoImplementedProperty; } set { notAutoImplementedProperty = value; } }

            public string CustomProperytChangedOldValue;
            string customProperytChanged;
            public virtual string CustomProperytChanged { get { return customProperytChanged; } set { customProperytChanged = value; } }
            protected void OnCustomProperytChanged() {
            }
            protected void OnCustomProperytChanged(string oldValue) {
                CustomProperytChangedOldValue = oldValue;
            }

            public virtual string PropertyChanging { get; set; }
            public string PropertyChangingNewValue;
            protected void MyPropertyChanging(string newValue) {
                Assert.AreNotEqual(newValue, PropertyChanging);
                PropertyChangingNewValue = newValue;
            }
        }

        [Test]
        public void OverridingPropertyTest_Metadata() {
            POCOViewModel_WithMetadata viewModel = ViewModelSource.Create<POCOViewModel_WithMetadata>();
            CheckNotBindableProperty(viewModel, x => x.NotBindableProperty, (vm, x) => vm.NotBindableProperty = x, "x", "y");
            CheckBindableProperty(viewModel, x => x.NotAutoImplementedProperty, (vm, x) => vm.NotAutoImplementedProperty = x, "x", "y");
            CheckBindableProperty(viewModel, x => x.CustomProperytChanged, (vm, x) => vm.CustomProperytChanged = x, "x", "y", (x, val) => Assert.AreEqual(val, x.CustomProperytChangedOldValue));

            viewModel.PropertyChanging = null;
            viewModel.PropertyChanging = "x";
            Assert.AreEqual("x", viewModel.PropertyChangingNewValue);
        }

        [Test]
        public void OverridingPropertyTest_Metadata_FluentAPI() {
            POCOViewModel_WithMetadata_FluentAPI viewModel = ViewModelSource.Create<POCOViewModel_WithMetadata_FluentAPI>();
            CheckNotBindableProperty(viewModel, x => x.NotBindableProperty, (vm, x) => vm.NotBindableProperty = x, "x", "y");
            CheckBindableProperty(viewModel, x => x.NotAutoImplementedProperty, (vm, x) => vm.NotAutoImplementedProperty = x, "x", "y");
            CheckBindableProperty(viewModel, x => x.CustomProperytChanged, (vm, x) => vm.CustomProperytChanged = x, "x", "y", (x, val) => Assert.AreEqual(val, x.CustomProperytChangedOldValue));

            viewModel.PropertyChanging = null;
            viewModel.PropertyChanging = "x";
            Assert.AreEqual("x", viewModel.PropertyChangingNewValue);
        }
        #endregion

        #region IPOCOViewModelImplementation
        [Test]
        public void IPOCOViewModelImplementation() {
            var viewModel = ViewModelSource.Create<POCOViewModel>();
            string propertyName = null;
            ((INotifyPropertyChanged)viewModel).PropertyChanged += (o, e) => propertyName = e.PropertyName;
            ((IPOCOViewModel)viewModel).RaisePropertyChanged("Property1");
            Assert.AreEqual("Property1", propertyName);

            viewModel.RaisePropertyChanged(x => x.Property5);
            Assert.AreEqual("Property5", propertyName);

            viewModel.RaisePropertiesChanged();
            Assert.AreEqual(string.Empty, propertyName);
        }
        [Test]
        public void GetSetParentViewModel() {
            var viewModel = ViewModelSource.Create<POCOViewModel>();
            Assert.IsNull(viewModel.GetParentViewModel<Button>());
            var b = new Button();
            Assert.AreSame(viewModel, viewModel.SetParentViewModel(b));
            Assert.AreEqual(b, viewModel.GetParentViewModel<Button>());
        }
        #endregion

        #region commands
        public class ProtecteCanExecuteMethod {
            public bool IsMethod1Enabled;
            public void Method1() {
                MessageBox.Show("Hello");
            }
            protected bool CanMethod1() {
                return IsMethod1Enabled;
            }

            public bool IsMethod2Enabled;
            public void Method2() {
                MessageBox.Show("Hello");
            }
            protected internal bool CanMethod2() {
                return IsMethod2Enabled;
            }
        }
        [Test]
        public void ProtectedCanExecuteMethod() {
            var viewModel = ViewModelSource.Create<ProtecteCanExecuteMethod>();
            viewModel
                .IsFalse(x => x.GetCommand(y => y.Method1()).CanExecute(null))
                .Do(x => x.IsMethod1Enabled = true)
                .IsTrue(x => x.GetCommand(y => y.Method1()).CanExecute(null))

                .IsFalse(x => x.GetCommand(y => y.Method2()).CanExecute(null))
                .Do(x => x.IsMethod2Enabled = true)
                .IsTrue(x => x.GetCommand(y => y.Method2()).CanExecute(null))

                ;
        }

        [CLSCompliant(false)]
        public class POCOCommandsViewModel {
            public virtual string Property1 { get; set; }

            protected int _ShowCommand;
            public int ShowCallCount;
            public void Show() {
                ShowCallCount++;
            }
            public int ShowAsyncCallCount;

            public Task ShowAsync() {
                return Task.Factory.StartNew(() =>
                    ShowAsyncCallCount++
                );
            }

            public int SaveCallCount;
            public void Save() {
                SaveCallCount++;
            }
            public int CloseCallCount;
            public void Close(string param) {
                CloseCallCount++;
            }

            public static void StaticMethod() { }
            internal void InternalMethod() { }
            protected Task ProtectedAsyncMethod() { return null; }
            protected void ProtectedMethod() { }
            public void OutParameter(out int x) { x = 0; }
            public void RefParameter(ref int x) { x = 0; }
            public int MethodWithReturnValue() { return 0; }
        }
        [Test]
        public void CommandsGeneration() {
            POCOCommandsViewModel viewModel = ViewModelSource.Create<POCOCommandsViewModel>();
            CheckCommand(viewModel, x => x.Show(), x => Assert.AreEqual(1, x.ShowCallCount));
            CheckCommand(viewModel, x => x.ShowAsync(), x => Assert.AreEqual(1, x.ShowAsyncCallCount), true);
            CheckCommand(viewModel, x => x.Save(), x => Assert.AreEqual(1, x.SaveCallCount));
            CheckCommand(viewModel, x => x.Close(null), x => Assert.AreEqual(1, x.CloseCallCount));
            CheckNoCommand(viewModel, "InternalMethod");
            CheckNoCommand(viewModel, "ToString");
            CheckNoCommand(viewModel, "GetHashCode");
            CheckNoCommand(viewModel, "Equals");
            CheckNoCommand(viewModel, "ProtectedAsyncMethod");
            CheckNoCommand(viewModel, "ProtectedMethod");
            CheckNoCommand(viewModel, "get_Property1");
            CheckNoCommand(viewModel, "set_Property1");
            CheckNoCommand(viewModel, "StaticMethod");
            CheckNoCommand(viewModel, "OutParameter");
            CheckNoCommand(viewModel, "RefParameter");
            CheckNoCommand(viewModel, "MethodWithReturnValue");

            Assert.AreEqual(typeof(ICommand), viewModel.GetType().GetProperty("ShowCommand").PropertyType);
            Assert.AreEqual(typeof(DelegateCommand<string>), viewModel.GetType().GetProperty("CloseCommand").PropertyType);
            Assert.AreEqual(typeof(AsyncCommand), viewModel.GetType().GetProperty("ShowAsyncCommand").PropertyType);
        }

        public class POCOCommandsCanExecute {
            public int ShowCallCount;
            public void Show() {
                ShowCallCount++;
            }
            public bool CanShowValue;
            public bool CanShow() {
                return CanShowValue;
            }

            public int OpenCallCount;
            public string OpenLastParameter;
            public void Open(string parameter) {
                OpenCallCount++;
                OpenLastParameter = parameter;
            }
            public bool CanOpen(string parameter) {
                return parameter != "x";
            }

            public int CloseCallCount;
            public int CloseLastParameter;
            public void Close(int parameter) {
                CloseCallCount++;
                CloseLastParameter = parameter;
            }
            public bool CanClose(int parameter) {
                return parameter != 9;
            }
        }
        [Test]
        public void CommandsCanExecute() {
            POCOCommandsCanExecute viewModel = ViewModelSource.Create<POCOCommandsCanExecute>();
            ICommand command = (ICommand)TypeHelper.GetPropertyValue(viewModel, "ShowCommand");
            Assert.IsFalse(command.CanExecute(null));
            viewModel.CanShowValue = true;
            Assert.IsTrue(command.CanExecute(null));

            command = (ICommand)TypeHelper.GetPropertyValue(viewModel, "OpenCommand");
            Assert.IsTrue(command.CanExecute("y"));
            Assert.IsFalse(command.CanExecute("x"));
            Assert.AreEqual(0, viewModel.OpenCallCount);
            command.Execute("z");
            Assert.AreEqual("z", viewModel.OpenLastParameter);
            Assert.AreEqual(1, viewModel.OpenCallCount);

            command = (ICommand)TypeHelper.GetPropertyValue(viewModel, "CloseCommand");
            Assert.IsFalse(command.CanExecute(9));
            Assert.IsTrue(command.CanExecute(13));
            Assert.IsFalse(command.CanExecute("9"));
            Assert.IsTrue(command.CanExecute("13"));
            Assert.AreEqual(0, viewModel.CloseCallCount);
            command.Execute("117");
            Assert.AreEqual(117, viewModel.CloseLastParameter);
            Assert.AreEqual(1, viewModel.CloseCallCount);
        }
        public class POCOAsyncCommands {
            public Task Show() {
                return null;
            }
            public bool CanShowValue;
            public bool CanShow() {
                return CanShowValue;
            }
            [AsyncCommand(AllowMultipleExecution = true)]
            public Task Open(string parameter) {
                return null;
            }
            public bool CanOpen(string parameter) {
                return parameter != "x";
            }
        }
        [Test]
        public void AsyncCommandsCanExecute() {
            POCOAsyncCommands viewModel = ViewModelSource.Create<POCOAsyncCommands>();
            IAsyncCommand asyncCommand = (IAsyncCommand)TypeHelper.GetPropertyValue(viewModel, "ShowCommand");
            Assert.IsFalse(asyncCommand.CanExecute(null));
            viewModel.CanShowValue = true;
            Assert.IsTrue(asyncCommand.CanExecute(null));

            asyncCommand = (IAsyncCommand)TypeHelper.GetPropertyValue(viewModel, "OpenCommand");
            Assert.IsTrue(asyncCommand.CanExecute("y"));
            Assert.IsFalse(asyncCommand.CanExecute("x"));
        }
        [Test]
        public void AsyncCommandAllowMultipleExecutionAttributeTest() {
            POCOAsyncCommands viewModel = ViewModelSource.Create<POCOAsyncCommands>();
            AsyncCommand asyncCommand1 = (AsyncCommand)TypeHelper.GetPropertyValue(viewModel, "ShowCommand");
            Assert.IsFalse(asyncCommand1.AllowMultipleExecution);
            AsyncCommand<string> asyncCommand2 = (AsyncCommand<string>)TypeHelper.GetPropertyValue(viewModel, "OpenCommand");
            Assert.IsTrue(asyncCommand2.AllowMultipleExecution);
        }

        public abstract class CommandAttributeViewModelBaseCounters {
            public int BaseClassCommandCallCount;
            public int SimpleMethodCallCount;
            public int MethodWithCommandCallCount;
            public int CustomNameCommandCallCount;
            public bool MethodWithCanExecuteCanExcute = false;
            public int MethodWithReturnTypeCallCount;
            public int MethodWithReturnTypeAndParameterCallCount;
            public int MethodWithParameterCallCount;
            public int MethodWithParameterLastParameter;
            public bool MethodWithCustomCanExecuteCanExcute = false;
        }
        public abstract class CommandAttributeViewModelBase : CommandAttributeViewModelBaseCounters {
            public void BaseClass() { BaseClassCommandCallCount++; }
        }
        public class CommandAttributeViewModel : CommandAttributeViewModelBase {
            public void Simple() { SimpleMethodCallCount++; }
            public void MethodWith() { MethodWithCommandCallCount++; }

            [Command(false)]
            public void NoAttribute() { }

            [Command(Name = "MyCommand")]
            public void CustomName() { CustomNameCommandCallCount++; }

            public void MethodWithCanExecute() { }
            public bool CanMethodWithCanExecute() { return MethodWithCanExecuteCanExcute; }

            [Command]
            protected int MethodWithReturnType() { MethodWithReturnTypeCallCount++; return 0; }
            [Command]
            public int MethodWithReturnTypeAndParameter(string param) { Assert.AreEqual("x", param); MethodWithReturnTypeAndParameterCallCount++; return 0; }

            public void MethodWithParameter(int parameter) { MethodWithParameterCallCount++; MethodWithParameterLastParameter = parameter; }
            public bool CanMethodWithParameter(int parameter) { return parameter != 13; }

            [Command(CanExecuteMethodName = "CanMethodWithCustomCanExecute_"
#if !SILVERLIGHT
, UseCommandManager = false
#endif
)]
            public void MethodWithCustomCanExecute() { }
            public bool CanMethodWithCustomCanExecute_() { return MethodWithCustomCanExecuteCanExcute; }
        }
        public class AsyncCommandAttributeViewModel : CommandAttributeViewModelBase {
            public Task Simple() {
                return Task.Factory.StartNew(() => SimpleMethodCallCount++);
            }
            public Task MethodWith() {
                return Task.Factory.StartNew(() => MethodWithCommandCallCount++);
            }

            [AsyncCommand(false)]
            public Task NoAttribute() { return null; }

            [AsyncCommand(Name = "MyCommand")]
            public Task CustomName() {
                return Task.Factory.StartNew(() => CustomNameCommandCallCount++);

            }

            public Task MethodWithCanExecute() { return null; }
            public bool CanMethodWithCanExecute() { return MethodWithCanExecuteCanExcute; }

            public Task MethodWithParameter(int parameter) {
                return Task.Factory.StartNew(() => {
                    MethodWithParameterCallCount++;
                    MethodWithParameterLastParameter = parameter;
                });
            }
            public bool CanMethodWithParameter(int parameter) { return parameter != 13; }

            [Command(CanExecuteMethodName = "CanMethodWithCustomCanExecute_"
#if !SILVERLIGHT
, UseCommandManager = false
#endif
)]
            public Task MethodWithCustomCanExecute() { return null; }
            public bool CanMethodWithCustomCanExecute_() { return MethodWithCustomCanExecuteCanExcute; }
        }
        [MetadataType(typeof(Metadata))]
        public class CommandAttributeViewModel_FluentAPI : CommandAttributeViewModelBaseCounters {
            public class Metadata : IMetadataProvider<CommandAttributeViewModel_FluentAPI> {
                void IMetadataProvider<CommandAttributeViewModel_FluentAPI>.BuildMetadata(MetadataBuilder<CommandAttributeViewModel_FluentAPI> builder) {
                    builder.CommandFromMethod(x => x.NoAttribute()).DoNotCreateCommand();
                    builder.CommandFromMethod(x => x.CustomName()).CommandName("MyCommand");
                    builder.CommandFromMethod(x => x.MethodWithReturnType());
                    builder.CommandFromMethod(x => x.MethodWithReturnTypeAndParameter(null));
                    builder.CommandFromMethod(x => x.MethodWithCustomCanExecute())
#if !SILVERLIGHT
.DoNotUseCommandManager()
#endif
.CanExecuteMethod(x => x.CanMethodWithCustomCanExecute_())
#if !SILVERLIGHT
.DoNotUseCommandManager()
#endif
;
                }
            }

            public void BaseClass() { BaseClassCommandCallCount++; }
            public void Simple() { SimpleMethodCallCount++; }
            public void MethodWith() { MethodWithCommandCallCount++; }
            public void NoAttribute() { }
            public void CustomName() { CustomNameCommandCallCount++; }
            public void MethodWithCanExecute() { }
            public bool CanMethodWithCanExecute() { return MethodWithCanExecuteCanExcute; }
            protected internal int MethodWithReturnType() { MethodWithReturnTypeCallCount++; return 0; }
            public int MethodWithReturnTypeAndParameter(string param) { Assert.AreEqual("x", param); MethodWithReturnTypeAndParameterCallCount++; return 0; }
            public void MethodWithParameter(int parameter) { MethodWithParameterCallCount++; MethodWithParameterLastParameter = parameter; }
            public bool CanMethodWithParameter(int parameter) { return parameter != 13; }
            public void MethodWithCustomCanExecute() { }

            public bool CanMethodWithCustomCanExecute_(int x) { throw new InvalidOperationException(); }
            public bool CanMethodWithCustomCanExecute_() { return MethodWithCustomCanExecuteCanExcute; }
        }
        [MetadataType(typeof(Metadata))]
        public class CommandAttributeViewModel_ExternalMetadata : CommandAttributeViewModelBaseCounters {
            public class Metadata {
                [Command(false)]
                public void NoAttribute() { }
                [Command(Name = "MyCommand")]
                public void CustomName() { }
                [Command]
                public void MethodWithReturnType() { }
                [Command]
                public void MethodWithReturnTypeAndParameter() { }
                [Command(CanExecuteMethodName = "CanMethodWithCustomCanExecute_"
#if !SILVERLIGHT
, UseCommandManager = false
#endif
)]
                public void MethodWithCustomCanExecute() { }
            }

            public void BaseClass() { BaseClassCommandCallCount++; }
            public void Simple() { SimpleMethodCallCount++; }
            public void MethodWith() { MethodWithCommandCallCount++; }
            public void NoAttribute() { }
            public void CustomName() { CustomNameCommandCallCount++; }
            public void MethodWithCanExecute() { }
            public bool CanMethodWithCanExecute() { return MethodWithCanExecuteCanExcute; }
            protected internal int MethodWithReturnType() { MethodWithReturnTypeCallCount++; return 0; }
            public int MethodWithReturnTypeAndParameter(string param) { Assert.AreEqual("x", param); MethodWithReturnTypeAndParameterCallCount++; return 0; }
            public void MethodWithParameter(int parameter) { MethodWithParameterCallCount++; MethodWithParameterLastParameter = parameter; }
            public bool CanMethodWithParameter(int parameter) { return parameter != 13; }
            public void MethodWithCustomCanExecute() { }

            public bool CanMethodWithCustomCanExecute_() { return MethodWithCustomCanExecuteCanExcute; }
        }
        [Test, Asynchronous]
        public void CommandAttribute_ViewModelTest() {
            var viewModel = ViewModelSource.Create<CommandAttributeViewModel>();
            CommandAttribute_ViewModelTestCore(viewModel, x => viewModel.MethodWithCanExecute(), x => viewModel.MethodWithCustomCanExecute());
        }
        [Test, Asynchronous]
        public void CommandAttribute_ViewModelTest_FluentAPI() {
            var viewModel = ViewModelSource.Create<CommandAttributeViewModel_FluentAPI>();
            CommandAttribute_ViewModelTestCore(viewModel, x => viewModel.MethodWithCanExecute(), x => viewModel.MethodWithCustomCanExecute());
        }
        [Test, Asynchronous]
        public void CommandAttribute_ViewModelTest_ExternalMetadata() {
            var viewModel = ViewModelSource.Create<CommandAttributeViewModel_ExternalMetadata>();
            CommandAttribute_ViewModelTestCore(viewModel, x => viewModel.MethodWithCanExecute(), x => viewModel.MethodWithCustomCanExecute());
        }
        [Test, Asynchronous]
        public void AsyncCommandAttribute_ViewModelTest() {
            var viewModel = ViewModelSource.Create<AsyncCommandAttributeViewModel>();
            CommandAttribute_ViewModelTestCore(viewModel, x => viewModel.MethodWithCanExecute(), x => viewModel.MethodWithCustomCanExecute(), true);
        }

        void CommandAttribute_ViewModelTestCore(CommandAttributeViewModelBaseCounters viewModel, Expression<Action<CommandAttributeViewModelBaseCounters>> methodWithCanExecuteExpression, Expression<Action<CommandAttributeViewModelBaseCounters>> methodWithCustomCanExecuteExpression, bool IsAsyncCommand = false) {
            Button button = null;
            EnqueueCallback(() => {
                button = new Button() { DataContext = viewModel };
                button.SetBinding(Button.CommandProperty, new Binding("SimpleCommand"));
                button.Command.Execute(null);
            });
            EnqueueWait(() => viewModel.SimpleMethodCallCount == 1);
            EnqueueCallback(() => {
                button.SetBinding(Button.CommandProperty, new Binding("NoAttributeCommand"));
                Assert.IsNull(button.Command);

                button.SetBinding(Button.CommandProperty, new Binding("MethodWithCommand"));
                button.Command.Execute(null);
            });
            EnqueueWait(() => viewModel.MethodWithCommandCallCount == 1);
            EnqueueCallback(() => {
                button.SetBinding(Button.CommandProperty, new Binding("MyCommand"));
                button.Command.Execute(null);
            });
            EnqueueWait(() => viewModel.CustomNameCommandCallCount == 1);
            EnqueueCallback(() => {
                button.SetBinding(Button.CommandProperty, new Binding("BaseClassCommand"));
                button.Command.Execute(null);
                Assert.AreEqual(1, viewModel.BaseClassCommandCallCount);
                Assert.IsTrue(button.IsEnabled);

                button.SetBinding(Button.CommandProperty, new Binding("MethodWithCanExecuteCommand"));
                Assert.IsFalse(button.IsEnabled, "0");
                viewModel.MethodWithCanExecuteCanExcute = true;
            });
#if !SILVERLIGHT
            EnqueueWindowUpdateLayout(DispatcherPriority.Normal);
#endif
            EnqueueCallback(() => {
                Assert.IsFalse(button.IsEnabled, "1");
                viewModel.RaiseCanExecuteChanged(methodWithCanExecuteExpression);
                Assert.AreEqual(button.Command, viewModel.GetCommand(methodWithCanExecuteExpression));
#if !SILVERLIGHT
                Assert.IsFalse(button.IsEnabled, "2");
#endif
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.IsTrue(button.IsEnabled);
                if(!IsAsyncCommand) {
                    button.SetBinding(Button.CommandProperty, new Binding("MethodWithReturnTypeCommand"));
                    button.Command.Execute(null);
                    Assert.AreEqual(1, viewModel.MethodWithReturnTypeCallCount);

                    button.SetBinding(Button.CommandProperty, new Binding("MethodWithReturnTypeAndParameterCommand"));
                    button.Command.Execute("x");
                    Assert.AreEqual(1, viewModel.MethodWithReturnTypeAndParameterCallCount);
                }

                button.SetBinding(Button.CommandProperty, new Binding("MethodWithParameterCommand"));
                button.Command.Execute(9);
            });
            EnqueueWait(() => viewModel.MethodWithParameterCallCount == 1);
            EnqueueCallback(() => {
                if(button.Command is IAsyncCommand)
                    EnqueueWait(() => ((IAsyncCommand)button.Command).IsExecuting == false);
            });
            EnqueueCallback(() => {
                Assert.AreEqual(9, viewModel.MethodWithParameterLastParameter);
                Assert.IsTrue(button.Command.CanExecute(9));
                Assert.IsFalse(button.Command.CanExecute(13), "3");
                button.Command.Execute("10");
            });
            EnqueueWait(() => viewModel.MethodWithParameterCallCount == 2);
            EnqueueCallback(() => {
                Assert.AreEqual(2, viewModel.MethodWithParameterCallCount);
                Assert.AreEqual(10, viewModel.MethodWithParameterLastParameter);

                button.SetBinding(Button.CommandProperty, new Binding("MethodWithCustomCanExecuteCommand"));
                Assert.IsFalse(button.IsEnabled, "4");
                viewModel.MethodWithCustomCanExecuteCanExcute = true;
                Assert.IsFalse(button.IsEnabled, "5");
                viewModel.RaiseCanExecuteChanged(methodWithCustomCanExecuteExpression);
                Assert.IsTrue(button.IsEnabled);
            });
            EnqueueTestComplete();
        }

        public class POCOViewModel_CommandsInViewModelBaseDescendant : ViewModelBase {
            [Command]
            public void Save() { SaveCallCount++; }
            public int SaveCallCount;
        }
        [Test]
        public void CommandsInViewModelBaseDescendant() {
            var viewModel = ViewModelSource.Create<POCOViewModel_CommandsInViewModelBaseDescendant>();
            var command = CheckCommand(viewModel, x => x.Save(), x => Assert.AreEqual(1, x.SaveCallCount));
            Assert.IsNotNull(TypeHelper.GetPropertyValue(viewModel, "SaveCommand"));
            int canExecuteChangedCount = 0;
            command.CanExecuteChanged += (x, e) => canExecuteChangedCount++;
            viewModel.RaiseCanExecuteChanged(() => viewModel.Save());
#if !SILVERLIGHT
            DispatcherHelper.DoEvents();
#endif
            Assert.AreEqual(1, canExecuteChangedCount);
        }
        public class POCOViewModel_AsyncCommandsInViewModelBaseDescendant : ViewModelBase {
            public Task Save() {
                return Task.Factory.StartNew(() => SaveCallCount++);
            }
            public int SaveCallCount;
        }
        [Test]
        public void AsyncCommandsInViewModelBaseDescendant() {
            var viewModel = ViewModelSource.Create<POCOViewModel_AsyncCommandsInViewModelBaseDescendant>();
            var command = CheckCommand(viewModel, x => x.Save(), x => Assert.AreEqual(1, x.SaveCallCount), true);
            Assert.IsNotNull(TypeHelper.GetPropertyValue(viewModel, "SaveCommand"));
            int canExecuteChangedCount = 0;
            command.CanExecuteChanged += (x, e) => canExecuteChangedCount++;
            viewModel.RaiseCanExecuteChanged(() => viewModel.Save());
#if !SILVERLIGHT
            DispatcherHelper.DoEvents();
#endif
            Assert.AreEqual(1, canExecuteChangedCount);
        }
        #endregion
        #region non default constructors
        public class POCOViewModel_NonDefaultConstructors {
            public string Property1 { get; set; }
            public string Property2 { get; set; }

            public string ConstructorInfo;
            public POCOViewModel_NonDefaultConstructors() {
                ConstructorInfo = "public";
            }
            public POCOViewModel_NonDefaultConstructors(int x) {
                ConstructorInfo = "public with param: " + x;
            }
            protected POCOViewModel_NonDefaultConstructors(string x) {
                ConstructorInfo = "protected: " + x;
            }
            protected internal POCOViewModel_NonDefaultConstructors(int x, string y) {
                ConstructorInfo = "protected internal: " + x + y;
            }
            POCOViewModel_NonDefaultConstructors(int x, int y) {
                throw new InvalidOperationException();
            }
            internal POCOViewModel_NonDefaultConstructors(int x, int y, int z) {
                throw new InvalidOperationException();
            }
        }
        [Test]
        public void NonDefaultConstructors1() {
            Type type = ViewModelSource.GetPOCOType(typeof(POCOViewModel_NonDefaultConstructors));

            var viewModel1 = (POCOViewModel_NonDefaultConstructors)Activator.CreateInstance(type);
            Assert.AreEqual("public", viewModel1.ConstructorInfo);

            viewModel1 = (POCOViewModel_NonDefaultConstructors)Activator.CreateInstance(type, new object[] { 13 });
            Assert.AreEqual("public with param: 13", viewModel1.ConstructorInfo);

            viewModel1 = (POCOViewModel_NonDefaultConstructors)Activator.CreateInstance(type, new object[] { "x" });
            Assert.AreEqual("protected: x", viewModel1.ConstructorInfo);

            viewModel1 = (POCOViewModel_NonDefaultConstructors)Activator.CreateInstance(type, new object[] { 9, "x" });
            Assert.AreEqual("protected internal: 9x", viewModel1.ConstructorInfo);

            Assert.IsNull(type.GetConstructor(new[] { typeof(int), typeof(int) }));
            Assert.IsNull(type.GetConstructor(new[] { typeof(int), typeof(int), typeof(int) }));
        }
        int fieldClojure = 117;
        int PropertyClojure = 253;
        [Test]
        public void NonDefaultConstructors2() {
            var viewModel1 = ViewModelSource.Create(() => new POCOViewModel_NonDefaultConstructors());
            Assert.AreEqual("public", viewModel1.ConstructorInfo);

            viewModel1 = ViewModelSource.Create(() => new POCOViewModel_NonDefaultConstructors() { Property1 = "p1", Property2 = "p2" });
            Assert.AreEqual("public", viewModel1.ConstructorInfo);
            Assert.AreEqual("p1", viewModel1.Property1);
            Assert.AreEqual("p2", viewModel1.Property2);

            viewModel1 = ViewModelSource.Create(() => new POCOViewModel_NonDefaultConstructors(13));
            Assert.AreEqual("public with param: 13", viewModel1.ConstructorInfo);

            viewModel1 = ViewModelSource.Create(() => new POCOViewModel_NonDefaultConstructors(7) { Property1 = "p1_", Property2 = "p2_" });
            Assert.AreEqual("public with param: 7", viewModel1.ConstructorInfo);
            Assert.AreEqual("p1_", viewModel1.Property1);
            Assert.AreEqual("p2_", viewModel1.Property2);

            var localVariableClojure = 9;
            viewModel1 = ViewModelSource.Create(() => new POCOViewModel_NonDefaultConstructors(localVariableClojure + 5));
            Assert.AreEqual("public with param: 14", viewModel1.ConstructorInfo);

            viewModel1 = ViewModelSource.Create(() => new POCOViewModel_NonDefaultConstructors(fieldClojure));
            Assert.AreEqual("public with param: 117", viewModel1.ConstructorInfo);

            viewModel1 = ViewModelSource.Create(() => new POCOViewModel_NonDefaultConstructors(PropertyClojure));
            Assert.AreEqual("public with param: 253", viewModel1.ConstructorInfo);

            viewModel1 = ViewModelSource.Create(() => new POCOViewModel_NonDefaultConstructors(9, "x"));
            Assert.AreEqual("protected internal: 9x", viewModel1.ConstructorInfo);

            viewModel1 = NonDefaultConstructors2Core(viewModel1, 13, "y");
        }
        public class POCOViewModel_NonDefaultConstructors_ProtectedDefaultCtor {
            public string ConstructorInfo;
            protected POCOViewModel_NonDefaultConstructors_ProtectedDefaultCtor() {
                ConstructorInfo = "public";
            }
        }
        private static POCOViewModel_NonDefaultConstructors NonDefaultConstructors2Core(POCOViewModel_NonDefaultConstructors viewModel1, int x, string y) {
            viewModel1 = ViewModelSource.Create(() => new POCOViewModel_NonDefaultConstructors(x, y));
            Assert.AreEqual("protected internal: 13y", viewModel1.ConstructorInfo);
            return viewModel1;
        }

        [Test]
        public void NonDefaultConstructors3() {
            Type type = ViewModelSource.GetPOCOType(typeof(POCOViewModel_NonDefaultConstructors_ProtectedDefaultCtor));
            var viewModel1 = (POCOViewModel_NonDefaultConstructors_ProtectedDefaultCtor)Activator.CreateInstance(type);
            Assert.AreEqual("public", viewModel1.ConstructorInfo);
        }
        public class POCOViewModel_NonDefaultConstructors_NoDefaultCtor {
            public string ConstructorInfo;
            POCOViewModel_NonDefaultConstructors_NoDefaultCtor() {
                ConstructorInfo = "public";
            }
            public POCOViewModel_NonDefaultConstructors_NoDefaultCtor(int x) {
                ConstructorInfo = "public with param: " + x;
            }
        }
        [Test]
        public void NonDefaultConstructors4() {
            var viewModel = ViewModelSource.Create(() => new POCOViewModel_NonDefaultConstructors_NoDefaultCtor(13));
            Assert.AreEqual("public with param: 13", viewModel.ConstructorInfo);
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
        public void GetFactoryTest() {
            var factory0 = ViewModelSource.Factory(() => new POCOViewModel_CreateViaGenericParameters());
            Assert.AreSame(factory0, ViewModelSource.Factory(() => new POCOViewModel_CreateViaGenericParameters()));
            var viewModel = factory0();
            Assert.AreEqual("", viewModel.ConstructorInfo);


            var factory1 = ViewModelSource.Factory((string p1) => new POCOViewModel_CreateViaGenericParameters(p1));
            Assert.AreSame(factory1, ViewModelSource.Factory((string p1) => new POCOViewModel_CreateViaGenericParameters(p1)));
            viewModel = factory1("x");
            Assert.AreEqual("x", viewModel.ConstructorInfo);
            viewModel = factory1("y");
            Assert.AreEqual("y", viewModel.ConstructorInfo);

            var factory2 = ViewModelSource.Factory((string p1, int p2) => new POCOViewModel_CreateViaGenericParameters(p1, p2));
            Assert.AreSame(factory2, ViewModelSource.Factory((string a, int b) => new POCOViewModel_CreateViaGenericParameters(a, b)));
            viewModel = factory2("x", 1);
            Assert.AreEqual("x1", viewModel.ConstructorInfo);

            var factory3 = ViewModelSource.Factory((string p1, int p2, bool p3) => new POCOViewModel_CreateViaGenericParameters(p1, p2, p3));
            viewModel = factory3("x", 1, true);
            Assert.AreEqual("x1True", viewModel.ConstructorInfo);

            var factory4 = ViewModelSource.Factory((string p1, int p2, bool p3, double p4) => new POCOViewModel_CreateViaGenericParameters(p1, p2, p3, p4));
            viewModel = factory4("x", 1, true, 2);
            Assert.AreEqual("x1True2", viewModel.ConstructorInfo);

            var factory5 = ViewModelSource.Factory((string p1, int p2, bool p3, double p4, char p5) => new POCOViewModel_CreateViaGenericParameters(p1, p2, p3, p4, p5));
            viewModel = factory5("x", 1, true, 2, 'y');
            Assert.AreEqual("x1True2y", viewModel.ConstructorInfo);

            var factory6 = ViewModelSource.Factory((string p1, int p2, bool p3, double p4, char p5, Visibility p6) => new POCOViewModel_CreateViaGenericParameters(p1, p2, p3, p4, p5, p6));
            viewModel = factory6("x", 1, true, 2, 'y', Visibility.Visible);
            Assert.AreEqual("x1True2yVisible", viewModel.ConstructorInfo);

            var factory7 = ViewModelSource.Factory((string p1, int p2, bool p3, double p4, char p5, Visibility p6, decimal p7) => new POCOViewModel_CreateViaGenericParameters(p1, p2, p3, p4, p5, p6, p7));
            viewModel = factory7("x", 1, true, 2, 'y', Visibility.Visible, 2);
            Assert.AreEqual("x1True2yVisible2", viewModel.ConstructorInfo);

            var factory8 = ViewModelSource.Factory((string p1, int p2, bool p3, double p4, char p5, Visibility p6, decimal p7, float p8) => new POCOViewModel_CreateViaGenericParameters(p1, p2, p3, p4, p5, p6, p7, p8));
            viewModel = factory8("x", 1, true, 2, 'y', Visibility.Visible, 2, 3);
            Assert.AreEqual("x1True2yVisible23", viewModel.ConstructorInfo);

            var factory9 = ViewModelSource.Factory((string p1, int p2, bool p3, double p4, char p5, Visibility p6, decimal p7, float p8, long p9) => new POCOViewModel_CreateViaGenericParameters(p1, p2, p3, p4, p5, p6, p7, p8, p9));
            viewModel = factory9("x", 1, true, 2, 'y', Visibility.Visible, 2, 3, 4);
            Assert.AreEqual("x1True2yVisible234", viewModel.ConstructorInfo);

            var factory10 = ViewModelSource.Factory((string p1, int p2, bool p3, double p4, char p5, Visibility p6, decimal p7, float p8, long p9, short p10) => new POCOViewModel_CreateViaGenericParameters(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10));
            viewModel = factory10("x", 1, true, 2, 'y', Visibility.Visible, 2, 3, 4, 5);
            Assert.AreEqual("x1True2yVisible2345", viewModel.ConstructorInfo);
        }
        [Test]
        public void CreateViaGenericParameters() {
            var viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create();
            Assert.AreEqual(string.Empty, viewModel.ConstructorInfo);

            viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create("x");
            Assert.AreEqual("x", viewModel.ConstructorInfo);

            viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create("x", 1);
            Assert.AreEqual("x1", viewModel.ConstructorInfo);

            viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create("x", 1, true);
            Assert.AreEqual("x1True", viewModel.ConstructorInfo);

            viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create("x", 1, true, 2.0);
            Assert.AreEqual("x1True2", viewModel.ConstructorInfo);

            viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create("x", 1, true, 2.0, 'y');
            Assert.AreEqual("x1True2y", viewModel.ConstructorInfo);

            viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create("x", 1, true, 2.0, 'y', Visibility.Visible);
            Assert.AreEqual("x1True2yVisible", viewModel.ConstructorInfo);

            viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create("x", 1, true, 2.0, 'y', Visibility.Visible, (decimal)2);
            Assert.AreEqual("x1True2yVisible2", viewModel.ConstructorInfo);

            viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create("x", 1, true, 2.0, 'y', Visibility.Visible, (decimal)2, (float)3);
            Assert.AreEqual("x1True2yVisible23", viewModel.ConstructorInfo);

            viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create("x", 1, true, 2.0, 'y', Visibility.Visible, (decimal)2, (float)3, (long)4);
            Assert.AreEqual("x1True2yVisible234", viewModel.ConstructorInfo);

            viewModel = ViewModelSource<POCOViewModel_CreateViaGenericParameters>.Create("x", 1, true, 2.0, 'y', Visibility.Visible, (decimal)2, (float)3, (long)4, (short)5);
            Assert.AreEqual("x1True2yVisible2345", viewModel.ConstructorInfo);
        }
        #endregion

        #region inheriting INotifyPropertyCanged
        public class POCOViewModel_BindableBaseDescendant : BindableBase {
            public virtual string Property1 { get; set; }

            string property2;
            public string Property2 {
                get { return property2; }
                set { SetProperty(ref property2, value, () => Property2); }
            }
        }
        [Test]
        public void InheritBindableBaseTest() {
            var viewModel = ViewModelSource.Create<POCOViewModel_BindableBaseDescendant>();
            var interfaces = viewModel.GetType().GetInterfaces();
            CheckBindableProperty(viewModel, x => x.Property1, (vm, x) => vm.Property1 = x, "x", "y");
            CheckBindableProperty(viewModel, x => x.Property2, (vm, x) => vm.Property2 = x, "x", "y");
        }
        public class POCOViewModel_INPCImplementorBase : INotifyPropertyChanged {
            public event PropertyChangedEventHandler PropertyChanged;
            protected void RaisePropertyChangedCore(string propertyName) {
                if(PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            public virtual void RaisePropertyChanged(string propertyName) {
            }
        }
        public class POCOViewModel_INPCImplementor : POCOViewModel_INPCImplementorBase {
            public virtual string Property1 { get; set; }
            public override void RaisePropertyChanged(string propertyName) {
                RaisePropertyChangedCore(propertyName);
            }
        }
        [Test]
        public void INPCImplementorTest() {
            var viewModel = ViewModelSource.Create<POCOViewModel_INPCImplementor>();
            var interfaces = viewModel.GetType().GetInterfaces();
            CheckBindableProperty(viewModel, x => x.Property1, (vm, x) => vm.Property1 = x, "x", "y");
        }
        #endregion

        #region services
        public class ParentViewModel : ViewModelBase { }
        public class SomeService { }
        public class POCOViewModel_ServicesViaCustomImplementationBase {
            public virtual IMessageBoxService MessageBoxSealed { get { return null; } }
        }
        public class POCOViewModel_ServicesViaCustomImplementation : POCOViewModel_ServicesViaCustomImplementationBase, ISupportServices {
            public IMessageBoxService MessageBoxNotVirtual { get { return ServiceContainer.GetService<IMessageBoxService>(); } }
            public virtual SomeService SomeService { get { return null; } }
            internal virtual IMessageBoxService MessageBoxInternal { get { return null; } }
            public sealed override IMessageBoxService MessageBoxSealed { get { return null; } }
            public virtual ISupportInitialize SupportInitialize { get { return null; } }
            protected internal virtual IMessageBoxService MessageBoxWithSetter { get; set; }

            public virtual IMessageBoxService MessageBox { get { return null; } }
            protected internal virtual IMessageBoxService MessageBoxProtectedInternal { get { return null; } }
            protected virtual ISplashScreenService SplashScreen { get { return null; } }
            public ISplashScreenService GetSplashScreen() { return SplashScreen; }
            [Required]
            public virtual IDocumentManagerService DocumentManager { get { return null; } }

            IServiceContainer serviceContainer;
            public IServiceContainer ServiceContainer {
                get { return serviceContainer ?? (serviceContainer = new ServiceContainer(this)); }
            }
        }
        [Test]
        public void ServicesTest() {
            var viewModel = ViewModelSource.Create<POCOViewModel_ServicesViaCustomImplementation>();
            Assert.IsNull(viewModel.SomeService);
            Assert.AreEqual(viewModel.GetType(), viewModel.GetType().GetProperty("MessageBox").DeclaringType);

            var messageBoxMock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            Assert.IsNull(viewModel.MessageBox);
            Assert.IsNull(viewModel.MessageBoxProtectedInternal);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService(messageBoxMock.Object);
            Assert.AreSame(messageBoxMock.Object, viewModel.MessageBox);
            Assert.AreSame(messageBoxMock.Object, viewModel.MessageBoxProtectedInternal);
            Assert.AreSame(messageBoxMock.Object, viewModel.MessageBoxNotVirtual);
            Assert.IsNull(viewModel.MessageBoxWithSetter);

            var splashScreenMock = new Mock<ISplashScreenService>(MockBehavior.Strict);
            Assert.IsNull(viewModel.GetSplashScreen());
            ((ISupportServices)viewModel).ServiceContainer.RegisterService(splashScreenMock.Object);
            Assert.AreSame(splashScreenMock.Object, viewModel.GetSplashScreen());

            var supportInitializeMock = new Mock<ISupportInitialize>(MockBehavior.Strict);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService(supportInitializeMock.Object);
            Assert.IsNull(viewModel.SupportInitialize);

            AssertHelper.AssertThrows<ServiceNotFoundException>(() => viewModel.DocumentManager.FindDocument(new object()));

            ParentViewModel parent = new ParentViewModel();
            var documentManagerServiceMock = new Mock<IDocumentManagerService>(MockBehavior.Strict);
            ((ISupportServices)parent).ServiceContainer.RegisterService(documentManagerServiceMock.Object);
            ((ISupportParentViewModel)viewModel).ParentViewModel = parent;
            Assert.AreSame(documentManagerServiceMock.Object, viewModel.DocumentManager);
        }
        public class POCOViewModel_Services : ISupportParentViewModel {
            public POCOViewModel_Services() {
                Assert.IsNotNull(((ISupportServices)this).ServiceContainer);
            }
            public virtual IMessageBoxService MessageBox { get { return null; } }
            public virtual ISplashScreenService SplashScreen { get { return null; } }

            public object ParentViewModel { get; set; }
        }
        [Test]
        public void ServicesTest_NoCustomImplementation() {
            var viewModel = ViewModelSource.Create<POCOViewModel_Services>();
            Assert.AreEqual(viewModel.GetType(), viewModel.GetType().GetProperty("MessageBox").DeclaringType);

            var messageBoxMock = new Mock<IMessageBoxService>(MockBehavior.Strict);
            Assert.IsNull(viewModel.MessageBox);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService(messageBoxMock.Object);
            Assert.AreSame(messageBoxMock.Object, viewModel.MessageBox);

            ParentViewModel parent = new ParentViewModel();
            var messageBoxMockParent = new Mock<IMessageBoxService>(MockBehavior.Strict);
            ((ISupportServices)parent).ServiceContainer.RegisterService(messageBoxMockParent.Object);
            viewModel.ParentViewModel = parent;
            Assert.AreSame(messageBoxMock.Object, viewModel.MessageBox);

            var splashScreenMockParent = new Mock<ISplashScreenService>(MockBehavior.Strict);
            Assert.IsNull(viewModel.SplashScreen);
            ((ISupportServices)parent).ServiceContainer.RegisterService(splashScreenMockParent.Object);
            Assert.AreSame(splashScreenMockParent.Object, viewModel.SplashScreen);
        }

        public class POCOViewModel_Services_Metadata {
            [ServiceProperty(Key = "MessageBox1")]
            public virtual IMessageBoxService MessageBox { get { return null; } }

            [ServiceProperty]
            public virtual ISplashScreenService SplashScreen { get { return null; } }

            [ServiceProperty(ServiceSearchMode.PreferParents)]
            public virtual ISplashScreenService SplashScreen_PreferParent { get { return null; } }

            [ServiceProperty(false)]
            public virtual IMessageBoxService NoMessageBox { get { return null; } }

            [ServiceProperty]
            public virtual ISupportInitialize SupportInitialize { get { return null; } }
        }
        [Test]
        public void ServicesTest_Metadata() {
            var viewModel = ViewModelSource.Create<POCOViewModel_Services_Metadata>();
            Assert.AreEqual(viewModel.GetType(), viewModel.GetType().GetProperty("MessageBox").DeclaringType);

            var messageBoxMock1 = new Mock<IMessageBoxService>(MockBehavior.Strict);
            var messageBoxMock2 = new Mock<IMessageBoxService>(MockBehavior.Strict);
            Assert.IsNull(viewModel.MessageBox);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService("x", messageBoxMock1.Object);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService("MessageBox1", messageBoxMock2.Object);
            Assert.AreSame(messageBoxMock2.Object, viewModel.MessageBox);
            Assert.IsNull(viewModel.NoMessageBox);

            ParentViewModel parent = new ParentViewModel();
            ((ISupportParentViewModel)viewModel).ParentViewModel = parent;
            var splashScreenMockParent = new Mock<ISplashScreenService>(MockBehavior.Strict);

            Assert.IsNull(viewModel.SplashScreen);
            ((ISupportServices)parent).ServiceContainer.RegisterService(splashScreenMockParent.Object);
            Assert.AreSame(splashScreenMockParent.Object, viewModel.SplashScreen);
            Assert.AreSame(splashScreenMockParent.Object, viewModel.SplashScreen_PreferParent);

            var splashScreenMock = new Mock<ISplashScreenService>(MockBehavior.Strict);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService(splashScreenMock.Object);
            Assert.AreSame(splashScreenMock.Object, viewModel.SplashScreen);
            Assert.AreSame(splashScreenMockParent.Object, viewModel.SplashScreen_PreferParent);

            var supportInitMock = new Mock<ISupportInitialize>(MockBehavior.Strict);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService(supportInitMock.Object);
            Assert.AreSame(supportInitMock.Object, viewModel.SupportInitialize);
        }

        [MetadataType(typeof(POCOViewModel_Services_Metadata_FluentAPIMetadata))]
        public class POCOViewModel_Services_Metadata_FluentAPI {
            public virtual IMessageBoxService MessageBox { get { return null; } }
            public virtual ISplashScreenService SplashScreen { get { return null; } }
            public virtual ISplashScreenService SplashScreen_PreferParent { get { return null; } }
            public virtual IMessageBoxService NoMessageBox { get { return null; } }
            public virtual ISupportInitialize SupportInitialize { get { return null; } }
        }
        public class POCOViewModel_Services_Metadata_FluentAPIMetadata : IMetadataProvider<POCOViewModel_Services_Metadata_FluentAPI> {
            void IMetadataProvider<POCOViewModel_Services_Metadata_FluentAPI>.BuildMetadata(MetadataBuilder<POCOViewModel_Services_Metadata_FluentAPI> builder) {
                builder.Property(x => x.MessageBox).ReturnsService("MessageBox1");
                builder.Property(x => x.SplashScreen).ReturnsService();
                builder.Property(x => x.SplashScreen_PreferParent).ReturnsService(ServiceSearchMode.PreferParents);
                builder.Property(x => x.NoMessageBox).DoesNotReturnService();
                builder.Property(x => x.SupportInitialize).ReturnsService();
            }
        }
        [MetadataType(typeof(POCOViewModel_Services_Metadata_ExternalMetadata))]
        public class POCOViewModel_Services_Metadata_External {
            public virtual IMessageBoxService MessageBox { get { return null; } }
            public virtual ISplashScreenService SplashScreen { get { return null; } }
            public virtual ISplashScreenService SplashScreen_PreferParent { get { return null; } }
            public virtual IMessageBoxService NoMessageBox { get { return null; } }
            public virtual ISupportInitialize SupportInitialize { get { return null; } }
        }
        public class POCOViewModel_Services_Metadata_ExternalMetadata {
            [ServiceProperty(Key = "MessageBox1")]
            public object MessageBox { get; set; }
            [ServiceProperty]
            public object SplashScreen { get; set; }
            [ServiceProperty(SearchMode = ServiceSearchMode.PreferParents)]
            public object SplashScreen_PreferParent { get; set; }
            [ServiceProperty(false)]
            public object NoMessageBox { get; set; }
            [ServiceProperty]
            public object SupportInitialize { get; set; }
        }
        [Test]
        public void ServicesTest_Metadata_FluentAPI() {
            var viewModel = ViewModelSource.Create<POCOViewModel_Services_Metadata_FluentAPI>();
            Assert.AreEqual(viewModel.GetType(), viewModel.GetType().GetProperty("MessageBox").DeclaringType);

            var messageBoxMock1 = new Mock<IMessageBoxService>(MockBehavior.Strict);
            var messageBoxMock2 = new Mock<IMessageBoxService>(MockBehavior.Strict);
            Assert.IsNull(viewModel.MessageBox);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService("x", messageBoxMock1.Object);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService("MessageBox1", messageBoxMock2.Object);
            Assert.AreSame(messageBoxMock2.Object, viewModel.MessageBox);
            Assert.IsNull(viewModel.NoMessageBox);

            ParentViewModel parent = new ParentViewModel();
            ((ISupportParentViewModel)viewModel).ParentViewModel = parent;
            var splashScreenMockParent = new Mock<ISplashScreenService>(MockBehavior.Strict);

            Assert.IsNull(viewModel.SplashScreen);
            ((ISupportServices)parent).ServiceContainer.RegisterService(splashScreenMockParent.Object);
            Assert.AreSame(splashScreenMockParent.Object, viewModel.SplashScreen);
            Assert.AreSame(splashScreenMockParent.Object, viewModel.SplashScreen_PreferParent);

            var splashScreenMock = new Mock<ISplashScreenService>(MockBehavior.Strict);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService(splashScreenMock.Object);
            Assert.AreSame(splashScreenMock.Object, viewModel.SplashScreen);
            Assert.AreSame(splashScreenMockParent.Object, viewModel.SplashScreen_PreferParent);

            var supportInitMock = new Mock<ISupportInitialize>(MockBehavior.Strict);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService(supportInitMock.Object);
            Assert.AreSame(supportInitMock.Object, viewModel.SupportInitialize);
        }
        [Test]
        public void ServicesTest_Metadata_External() {
            var viewModel = ViewModelSource.Create<POCOViewModel_Services_Metadata_External>();
            Assert.AreEqual(viewModel.GetType(), viewModel.GetType().GetProperty("MessageBox").DeclaringType);

            var messageBoxMock1 = new Mock<IMessageBoxService>(MockBehavior.Strict);
            var messageBoxMock2 = new Mock<IMessageBoxService>(MockBehavior.Strict);
            Assert.IsNull(viewModel.MessageBox);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService("x", messageBoxMock1.Object);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService("MessageBox1", messageBoxMock2.Object);
            Assert.AreSame(messageBoxMock2.Object, viewModel.MessageBox);
            Assert.IsNull(viewModel.NoMessageBox);

            ParentViewModel parent = new ParentViewModel();
            ((ISupportParentViewModel)viewModel).ParentViewModel = parent;
            var splashScreenMockParent = new Mock<ISplashScreenService>(MockBehavior.Strict);

            Assert.IsNull(viewModel.SplashScreen);
            ((ISupportServices)parent).ServiceContainer.RegisterService(splashScreenMockParent.Object);
            Assert.AreSame(splashScreenMockParent.Object, viewModel.SplashScreen);
            Assert.AreSame(splashScreenMockParent.Object, viewModel.SplashScreen_PreferParent);

            var splashScreenMock = new Mock<ISplashScreenService>(MockBehavior.Strict);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService(splashScreenMock.Object);
            Assert.AreSame(splashScreenMock.Object, viewModel.SplashScreen);
            Assert.AreSame(splashScreenMockParent.Object, viewModel.SplashScreen_PreferParent);

            var supportInitMock = new Mock<ISupportInitialize>(MockBehavior.Strict);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService(supportInitMock.Object);
            Assert.AreSame(supportInitMock.Object, viewModel.SupportInitialize);
        }

        public class GetServiceViewModel { }
        [Test]
        public void GetServiceMethodTest() {
            var viewModel = ViewModelSource.Create<GetServiceViewModel>();
            var parentViewModel = ViewModelSource.Create<GetServiceViewModel>();
            viewModel.SetParentViewModel(parentViewModel);

            Assert.IsNull(viewModel.GetService<IMessageBoxService>());
            TestHelper.AssertThrows<ServiceNotFoundException>(() => viewModel.GetRequiredService<IMessageBoxService>());

            var messageBoxMock1 = new Mock<IMessageBoxService>(MockBehavior.Strict);
            var messageBoxMock2 = new Mock<IMessageBoxService>(MockBehavior.Strict);

            ((ISupportServices)viewModel).ServiceContainer.RegisterService(messageBoxMock1.Object);
            ((ISupportServices)parentViewModel).ServiceContainer.RegisterService(messageBoxMock2.Object);
            Assert.AreSame(messageBoxMock1.Object, viewModel.GetService<IMessageBoxService>());
            Assert.AreSame(messageBoxMock1.Object, viewModel.GetRequiredService<IMessageBoxService>());
            TestHelper.AssertThrows<ViewModelSourceException>(() => new GetServiceViewModel().GetService<IMessageBoxService>());
        }
        [Test]
        public void GetServiceByNameMethodTest() {
            var viewModel = ViewModelSource.Create<GetServiceViewModel>();
            var parentViewModel = ViewModelSource.Create<GetServiceViewModel>();
            viewModel.SetParentViewModel(parentViewModel);

            Assert.IsNull(viewModel.GetService<IMessageBoxService>("svc1"));
            TestHelper.AssertThrows<ServiceNotFoundException>(() => viewModel.GetRequiredService<IMessageBoxService>("svc1"));

            var messageBoxMock1 = new Mock<IMessageBoxService>(MockBehavior.Strict);
            var messageBoxMock2 = new Mock<IMessageBoxService>(MockBehavior.Strict);
            var messageBoxMock3 = new Mock<IMessageBoxService>(MockBehavior.Strict);

            ((ISupportServices)viewModel).ServiceContainer.RegisterService("svc1", messageBoxMock1.Object);
            ((ISupportServices)viewModel).ServiceContainer.RegisterService("svc2", messageBoxMock2.Object);
            ((ISupportServices)parentViewModel).ServiceContainer.RegisterService("svc3", messageBoxMock3.Object);
            Assert.AreSame(messageBoxMock1.Object, viewModel.GetService<IMessageBoxService>("svc1"));
            Assert.AreSame(messageBoxMock2.Object, viewModel.GetService<IMessageBoxService>("svc2"));
            TestHelper.AssertThrows<ViewModelSourceException>(() => new GetServiceViewModel().GetService<IMessageBoxService>("svc1"));
        }
        #endregion

        #region IsInDesignMode
        [Test]
        public void IsInDesignModeTest() {
            POCOViewModel_PropertyChanged viewModel = ViewModelSource.Create<POCOViewModel_PropertyChanged>();
            Assert.IsFalse(viewModel.IsInDesignMode());
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            try {
                Assert.IsTrue(viewModel.IsInDesignMode());
            } finally {
                ViewModelDesignHelper.IsInDesignModeOverride = null;
            }
        }
        #endregion

        #region inheritance
        #endregion

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
        [Test]
        public void CreateByType() {
            Assert.AreEqual(13, ((IsPOCO_NoDefaultCtor1)ViewModelSourceHelper.Create(typeof(IsPOCO_NoDefaultCtor1))).X);
            Assert.AreEqual(0, ((IsPOCO_NoDefaultCtor2)ViewModelSourceHelper.Create(typeof(IsPOCO_NoDefaultCtor2))).X);
            Assert.AreEqual(0, ((IsPOCO_NoDefaultCtor3)ViewModelSourceHelper.Create(typeof(IsPOCO_NoDefaultCtor3))).X);
            AssertHelper.AssertThrows<ViewModelSourceException>(() => ViewModelSourceHelper.Create(typeof(IsPOCO_NoDefaultCtor4)));
            Assert.AreEqual(0, ((IsPOCO_NoDefaultCtor5)ViewModelSourceHelper.Create(typeof(IsPOCO_NoDefaultCtor5))).X);
            Assert.AreEqual(0, ((IsPOCO_NoDefaultCtor6)ViewModelSourceHelper.Create(typeof(IsPOCO_NoDefaultCtor6))).X);
        }
        [Test]
        public void IsPOCOViewModelTest() {
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_Empty)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_NotVirtualProperty)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_VirtualProperty_INPC)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_Method_Command)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_Method_Command2)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_Method_Sealed)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_Method_Command_Attribute_Sealed)));

            #region proeprties
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_InvalidMetadata_BindableAttributeOnNotVirtualProeprty)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithInternalSetter)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithoutSetter)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_InvalidMetadata_BindableAttributeOnProeprtyWithoutGetter)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_SealedClass)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_PrivateClass)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(InvalidIPOCOViewModelImplementation)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_INPCImplementor_NoPopertyChanged)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_INPCImplementor_PrivatePopertyChanged)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_INPCImplementor_ByRefPopertyChanged)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_INPCImplementor_OutPopertyChanged)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_INPCImplementor_NoArgPopertyChanged)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_FinalProperty)));
            #endregion

            #region commands
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_MemberWithCommandName)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_MemberWithCommandName2)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(DuplicateNamesViewModel)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(NotPublicMethodViewModel)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(TooMuchArgumentsMethodViewModel)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(OutParameterMethodViewModel)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(RefParameterMethodViewModel)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(RefParameterMethodViewModel)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(CanExecuteParameterCountMismatchViewModel)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(CanExecuteParametersMismatchViewModel)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(CanExecuteParametersMismatchViewModel2)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(NotPublicCanExecuteViewModel)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(InvalidCanExecuteMethodNameViewModel)));
            #endregion

            #region ctors and services
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(InternalCtor)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(OnlyInternalCtor)));

            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(Services_NotServiceType)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(Services_PropertyIsNotVirtual)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(Services_PropertyIsSealedBase)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(Services_PropertyHasSetter)));
            #endregion

            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_VirtualProperty)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_Method)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_Method_INPC)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_Method_Command_Attribute)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_PropertyChanged)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_PropertyChanging)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_WithMetadata)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_WithMetadata_FluentAPI)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOCommandsViewModel)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_VirtualProperty_NoDefaultCtor)));

            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOCommandsCanExecute)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(CommandAttributeViewModel)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(CommandAttributeViewModel_FluentAPI)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_CommandsInViewModelBaseDescendant)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_NonDefaultConstructors)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_NonDefaultConstructors_ProtectedDefaultCtor)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_NonDefaultConstructors_NoDefaultCtor)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_CreateViaGenericParameters)));

            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_BindableBaseDescendant)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_INPCImplementorBase)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_INPCImplementor)));

            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_ServicesViaCustomImplementation)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_Services)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_Services_Metadata)));
            Assert.AreEqual(false, ViewModelSourceHelper.IsPOCOViewModelType(typeof(POCOViewModel_Services_Metadata_FluentAPI)));

            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_NoDefaultCtor1)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_NoDefaultCtor2)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_NoDefaultCtor3)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_NoDefaultCtor5)));
            Assert.AreEqual(true, ViewModelSourceHelper.IsPOCOViewModelType(typeof(IsPOCO_NoDefaultCtor6)));
        }
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