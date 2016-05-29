using System.Collections.Generic;
using WhileTrue.Classes.Components.TestComponents;
#pragma warning disable 1591
// ReSharper disable InconsistentNaming
using System;
using NUnit.Framework;

namespace WhileTrue.Classes.Components
{ 
    [TestFixture]
    public class ComponentsTest
    {
        [Test]
        public void SimpleInstance_in_repository_must_be_retrievable_from_container()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade1 Test = Container.ResolveInstance<ITestFacade1>();
                Assert.IsNotNull(Test);
            }
        }

        [Test]
        public void SimpleInstance_shall_yield_the_same_instance_if_retrieved_twice()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1And2>(ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade1 Test1 = Container.ResolveInstance<ITestFacade1>();
                ITestFacade1 Test2 = Container.ResolveInstance<ITestFacade1>();
                Assert.AreSame(Test1, Test2);
            }
        }

        [Test]
        public void SimpleInstance_shall_yield_the_same_instance_if_retrieved_twice_indirectly_via_another_component()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Container);
            Repository.AddComponent<Test2>(ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade1 Test1 = Container.ResolveInstance<ITestFacade1>();
                ITestFacade2 Test2 = Container.ResolveInstance<ITestFacade2>();
                Assert.AreSame(Test1, ((Test2) Test2).TestFacade1);
            }
        }

        [Test]
        public void SimpleInstance_shall_yield_the_same_instance_if_retrieved_with_two_different_interface_types()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1And2>(ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade1 Test1 = Container.ResolveInstance<ITestFacade1>();
                ITestFacade2 Test2 = Container.ResolveInstance<ITestFacade2>();
                Assert.AreSame(Test1, Test2);
            }
        }

        [Test]
        public void SimpleInstance_must_be_creatable_with_non_optimal_constrcutor_if_dependencies_are_missing()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test2A>(ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Test2A Test = (Test2A) Container.ResolveInstance<ITestFacade2>();
                Assert.IsNull(Test.TestFacade1);
            }
        }

        [Test]
        public void SimpleInstance_must_be_created_with_optimal_constructor_when_all_dependencies_are_available()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Container);
            Repository.AddComponent<Test2A>(ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Test2A Test = (Test2A) Container.ResolveInstance<ITestFacade2>();
                Assert.IsNotNull(Test.TestFacade1);
            }
        }    
        
        [Test]
        public void Progress_shall_be_reported_for_hierarchical_object_creations()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Container);
            Repository.AddComponent<Test2A>(ComponentInstanceScope.Container);

            List<Tuple<Tuple<int,int>,string>> Progress = new List<Tuple<Tuple<int, int>, string>>();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Container.ResolveInstance<ITestFacade2>((max,cur,name)=>Progress.Add(new Tuple<Tuple<int, int>, string>(new Tuple<int, int>(max,cur),name)));
            }

            Assert.That(Progress.Count, Is.EqualTo(2) );
            Assert.That(Progress[0].Item1.Item1, Is.EqualTo(2) );
            Assert.That(Progress[0].Item1.Item2, Is.EqualTo(1) );
            Assert.That(Progress[0].Item2, Is.EqualTo("Test2A") );
            Assert.That(Progress[1].Item1.Item1, Is.EqualTo(2));
            Assert.That(Progress[1].Item1.Item2, Is.EqualTo(2));
            Assert.That(Progress[1].Item2, Is.EqualTo("Test1"));
        }

        [Test]
        public void SimpleInstance_must_be_initialized_with_config_when_created()
        {
            ComponentRepository Repository = new ComponentRepository();
            Config Config = new Config();
            Repository.AddComponent<ConfigTest1>(Config, ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {

                ITestFacade1 Test = Container.ResolveInstance<ITestFacade1>();
                Assert.AreEqual(Config, ((ConfigTest1) Test).Config);
            }
        }

        [Test]
        public void Instance_must_be_initialized_with_private_repository_if_explicitly_given_and_container_on_demand()
        {
            ComponentRepository Repository = new ComponentRepository();
            ComponentRepository PrivateRepository = new ComponentRepository();
            Repository.AddComponent<RepositoryParameterTest1>(PrivateRepository);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade1 Test = Container.ResolveInstance<ITestFacade1>();
                Assert.AreEqual(Container, ((RepositoryParameterTest1) Test).Container);
                Assert.AreEqual(PrivateRepository, ((RepositoryParameterTest1) Test).Repository);
            }
        }

        [Test]
        public void Instance_must_be_initialized_with_repository_and_container_on_demand()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RepositoryParameterTest1>();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade1 Test = Container.ResolveInstance<ITestFacade1>();
                Assert.AreEqual(Container, ((RepositoryParameterTest1)Test).Container);
                Assert.AreEqual(Repository, ((RepositoryParameterTest1)Test).Repository);
            }
        }


        [Test]
        public void SharedInstance_must_be_the_same_when_resolved_through_two_containers()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Repository);

            using( ComponentContainer Container1 = new ComponentContainer(Repository))
            using (ComponentContainer Container2 = new ComponentContainer(Repository))
            {


                ITestFacade1 TestFacade1 = Container1.ResolveInstance<ITestFacade1>();
                ITestFacade1 TestFacade2 = Container2.ResolveInstance<ITestFacade1>();

                Assert.IsNotNull(TestFacade1);
                Assert.IsNotNull(TestFacade2);
                Assert.AreEqual(TestFacade1, TestFacade2);
            }
        }

        [Test]
        public void SharedInstance_must_differ_when_resolved_via_two_containers_with_two_separate_repositories()
        {
            ComponentRepository Repository1 = new ComponentRepository();
            Repository1.AddComponent<Test1>(ComponentInstanceScope.Repository);

            ComponentRepository Repository2 = new ComponentRepository();
            Repository2.AddComponent<Test1>(ComponentInstanceScope.Repository);

            using(ComponentContainer Container1 = new ComponentContainer(Repository1))
            using (ComponentContainer Container2 = new ComponentContainer(Repository2))
            {

                ITestFacade1 TestFacade1 = Container1.ResolveInstance<ITestFacade1>();
                ITestFacade1 TestFacade2 = Container2.ResolveInstance<ITestFacade1>();

                Assert.AreNotEqual(TestFacade1, TestFacade2);
            }
        }

        [Test]
        public void SharedInstance_must_be_initialized_with_config_if_given()
        {
            ComponentRepository Repository = new ComponentRepository();
            Config Config = new Config();
            Repository.AddComponent<ConfigTest1>(Config, ComponentInstanceScope.Repository);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {

                ITestFacade1 Test = Container.ResolveInstance<ITestFacade1>();
                Assert.AreEqual(Config, ((ConfigTest1) Test).Config);
            }
        }

        [Test]
        public void SingletonInstance_must_be_the_same_when_resolved_through_two_containers()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Global);

            using( ComponentContainer Container1 = new ComponentContainer(Repository))
            using (ComponentContainer Container2 = new ComponentContainer(Repository))
            {


                ITestFacade1 TestFacade1 = Container1.ResolveInstance<ITestFacade1>();
                ITestFacade1 TestFacade2 = Container2.ResolveInstance<ITestFacade1>();

                Assert.IsNotNull(TestFacade1);
                Assert.IsNotNull(TestFacade2);
                Assert.AreEqual(TestFacade1, TestFacade2);
            }
        }

        [Test]
        public void SingletonInstance_must_be_the_same_when_resolved_through_two_containers_with_two_separate_repositories()
        {
            ComponentRepository Repository1 = new ComponentRepository();
            Repository1.AddComponent<Test1>(ComponentInstanceScope.Global);

            ComponentRepository Repository2 = new ComponentRepository();
            Repository2.AddComponent<Test1>(ComponentInstanceScope.Global);

            using(ComponentContainer Container1 = new ComponentContainer(Repository1))
            using (ComponentContainer Container2 = new ComponentContainer(Repository2))
            {
                ITestFacade1 TestFacade1 = Container1.ResolveInstance<ITestFacade1>();
                ITestFacade1 TestFacade2 = Container2.ResolveInstance<ITestFacade1>();

                Assert.IsNotNull(TestFacade1);
                Assert.IsNotNull(TestFacade2);
                Assert.AreEqual(TestFacade1, TestFacade2);
            }
        }

        [Test]
        public void SingeltonInstance_must_be_initialized_with_config_if_given()
        {
            ComponentRepository Repository = new ComponentRepository();
            Config Config = new Config();
            Repository.AddComponent<ConfigTest1>(Config, ComponentInstanceScope.Global);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {

                ITestFacade1 Test = Container.ResolveInstance<ITestFacade1>();
                Assert.AreEqual(Config, ((ConfigTest1) Test).Config);
            }
        }


        [Test]
        public void resolving_an_SimpleInstance_must_also_resolve_its_dependencies()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Container);
            Repository.AddComponent<Test2>(ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {

                ITestFacade2 Test = Container.ResolveInstance<ITestFacade2>();
                Assert.IsNotNull(Test);
                Assert.IsNotNull(((Test2) Test).TestFacade1);
            }
        }

        [Test]
        public void Disposing_the_container_shall_also_dispose_the_instance_simple()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Disposable>(ComponentInstanceScope.Container);
            
            Disposable Disposable;
            using(ComponentContainer Container = new ComponentContainer(Repository))
            {
                Disposable = (Disposable) Container.ResolveInstance<ITestFacade1>();
            }

            Assert.IsTrue(Disposable.IsDisposed);
        }

        [Test]
        public void Instance_shall_be_garbage_collected_after_one_and_only_reference_is_destroyed()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RepositoryParameterTest1>(ComponentInstanceScope.Container);

            WeakReference TestReference;

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                TestReference = new WeakReference(Container.ResolveInstance<ITestFacade1>());
                Assert.IsTrue(TestReference.IsAlive);
            }
            GC.Collect();
            Assert.IsFalse(TestReference.IsAlive);
        }

        [Test]
        public void Disposing_the_container_shall_also_dispose_the_instance_shared()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Disposable>(ComponentInstanceScope.Repository);

            Disposable Disposable;
            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Disposable = (Disposable)Container.ResolveInstance<ITestFacade1>();
            }

            Assert.IsTrue(Disposable.IsDisposed);
        }

        [Test]
        public void SharedInstance_shall_be_garbage_collected_after_one_and_only_reference_is_destroyed()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RepositoryParameterTest1>();

            WeakReference TestReference;

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                TestReference = new WeakReference(Container.ResolveInstance<ITestFacade1>());
                Assert.IsTrue(TestReference.IsAlive);
            }
            GC.Collect();
            Assert.IsFalse(TestReference.IsAlive);
        }

        [Test]
        public void SharedInstance_shall_be_garbage_collected_after_last_reference_is_destroyed()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RepositoryParameterTest1>();

            WeakReference TestReference;

            using (ComponentContainer Container1 = new ComponentContainer(Repository))
            using (ComponentContainer Container2 = new ComponentContainer(Repository))
            {
                Container1.ResolveInstance<ITestFacade1>();
                TestReference = new WeakReference(Container2.ResolveInstance<ITestFacade1>());
                Assert.IsTrue(TestReference.IsAlive);
            }
            GC.Collect();
            Assert.IsFalse(TestReference.IsAlive);
        }

        [Test]
        public void Disposing_the_container_shall_also_dispose_the_instance_singleton()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Disposable>(ComponentInstanceScope.Global);

            Disposable Disposable;
            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Disposable = (Disposable)Container.ResolveInstance<ITestFacade1>();
            }

            Assert.IsTrue(Disposable.IsDisposed);
        }

        [Test]
        public void SingletonInstance_shall_be_garbage_collected_after_one_and_only_reference_is_destroyed()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RepositoryParameterTest1>(ComponentInstanceScope.Global);

            WeakReference TestReference;

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                TestReference = new WeakReference(Container.ResolveInstance<ITestFacade1>());
                Assert.IsTrue(TestReference.IsAlive);
            }
            GC.Collect();
            Assert.IsFalse(TestReference.IsAlive);
        }

        [Test]
        public void SingletonInstance_shall_be_garbage_collected_after_last_reference_is_destroyed()
        {
            ComponentRepository Repository1 = new ComponentRepository();
            Repository1.AddComponent<RepositoryParameterTest1>(ComponentInstanceScope.Global);
            ComponentRepository Repository2 = new ComponentRepository();
            Repository2.AddComponent<RepositoryParameterTest1>(ComponentInstanceScope.Global);

            WeakReference TestReference;

            using (ComponentContainer Container1 = new ComponentContainer(Repository1))
            using (ComponentContainer Container2 = new ComponentContainer(Repository2))
            {
                Container1.ResolveInstance<ITestFacade1>();
                TestReference = new WeakReference(Container2.ResolveInstance<ITestFacade1>());
                Assert.IsTrue(TestReference.IsAlive);
            }
            GC.Collect();
            Assert.IsFalse(TestReference.IsAlive);
        }

        [Test]
        public void resolving_an_instance_shall_throw_an_exception_if_no_component_of_that_type_is_registered()
        {
            ComponentRepository Repository = new ComponentRepository();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Assert.Throws<ResolveComponentException>(() => Container.ResolveInstance<ITestFacade1>());
            }
        }


        [Test]
        public void resolving_an_instance_shall_throw_an_exception_if_multiple_components_of_that_type_are_registered()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>();
            Repository.AddComponent<ConfigTest1>(new Config());

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Assert.Throws<ResolveComponentException>(() => Container.ResolveInstance<ITestFacade1>());
            }
        }

        [Test]
        public void dependency_of_SimpleInstance_to_SharedInstance_must_also_work_through_container_borders()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Repository);
            Repository.AddComponent<Test2>(ComponentInstanceScope.Container);

            using( ComponentContainer Container1 = new ComponentContainer(Repository))
            using (ComponentContainer Container2 = new ComponentContainer(Repository))
            {

                ITestFacade1 TestFacade1 = Container1.ResolveInstance<ITestFacade1>();
                ITestFacade2 TestFacade2 = Container2.ResolveInstance<ITestFacade2>();
                Assert.IsNotNull(TestFacade2);
                Assert.IsNotNull(((Test2) TestFacade2).TestFacade1);
                Assert.AreEqual(TestFacade1, ((Test2) TestFacade2).TestFacade1);
            }
        }

        [Test]
        public void Two_singleton_components_shall_not_interfere_each_other()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Global);
            Repository.AddComponent<Test2C>(ComponentInstanceScope.Global);

            using( ComponentContainer Container1 = new ComponentContainer(Repository))
            using (ComponentContainer Container2 = new ComponentContainer(Repository))
            {

                ITestFacade1 TestFacade1 = Container1.ResolveInstance<ITestFacade1>();
                ITestFacade2 TestFacade2 = Container2.ResolveInstance<ITestFacade2>();

                Assert.IsInstanceOf<Test1>(TestFacade1);
                Assert.IsInstanceOf<Test2C>(TestFacade2);
            }
        }

        [Test]
        public void component_properties_shall_support_lazy_initialisation_when_depended_component_is_created_later_simple()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Container);
            Repository.AddComponent<Test2Lazy>(ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade2 TestFacade2 = Container.ResolveInstance<ITestFacade2>();
                Assert.IsNull(((Test2Lazy) TestFacade2).TestFacade1);

                Container.ResolveInstance<ITestFacade1>();
                Assert.IsNotNull(((Test2Lazy) TestFacade2).TestFacade1);
            }
        }

        [Test]
        public void component_properties_shall_support_lazy_initialisation_when_depended_component_is_created_later_shared()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>();
            Repository.AddComponent<Test2Lazy>();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade2 TestFacade2 = Container.ResolveInstance<ITestFacade2>();
                Assert.IsNull(((Test2Lazy) TestFacade2).TestFacade1);

                Container.ResolveInstance<ITestFacade1>();
                Assert.IsNotNull(((Test2Lazy) TestFacade2).TestFacade1);
            }
        }

        [Test]
        public void component_properties_shall_support_lazy_initialisation_when_depended_component_is_created_later_singleton()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Global);
            Repository.AddComponent<Test2Lazy>(ComponentInstanceScope.Global);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade2 TestFacade2 = Container.ResolveInstance<ITestFacade2>();
                Assert.IsNull(((Test2Lazy) TestFacade2).TestFacade1);

                Container.ResolveInstance<ITestFacade1>();
                Assert.IsNotNull(((Test2Lazy) TestFacade2).TestFacade1);
            }
        }
        [Test]
        public void component_properties_shall_support_lazy_initialisation_when_depended_component_is_already_created()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>(ComponentInstanceScope.Container);
            Repository.AddComponent<Test2Lazy>(ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Container.ResolveInstance<ITestFacade1>();
                ITestFacade2 TestFacade2 = Container.ResolveInstance<ITestFacade2>();

                Assert.IsNotNull(((Test2Lazy) TestFacade2).TestFacade1);
            }
        }

        [Test]
        public void component_properties_shall_support_lazy_initialisation_in_cyclic_dependency_scenario_component()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RecursionTest1>(ComponentInstanceScope.Container);
            Repository.AddComponent<Test2Lazy>(ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade1 TestFacade1 = Container.ResolveInstance<ITestFacade1>();
                ITestFacade2 TestFacade2 = Container.ResolveInstance<ITestFacade2>();

                Assert.AreEqual(TestFacade1, ((Test2Lazy) TestFacade2).TestFacade1);
            }
        }

        [Test]
        public void component_properties_shall_support_lazy_initialisation_in_cyclic_dependency_scenario_shared()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RecursionTest1>(ComponentInstanceScope.Repository);
            Repository.AddComponent<Test2Lazy>(ComponentInstanceScope.Repository);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade1 TestFacade1 = Container.ResolveInstance<ITestFacade1>();
                ITestFacade2 TestFacade2 = Container.ResolveInstance<ITestFacade2>();

                Assert.AreEqual(TestFacade1, ((Test2Lazy) TestFacade2).TestFacade1);
            }
        }

        [Test]
        public void component_properties_shall_support_lazy_initialisation_in_cyclic_dependency_scenario_singleton()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RecursionTest1>(ComponentInstanceScope.Global);
            Repository.AddComponent<Test2Lazy>(ComponentInstanceScope.Global);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade1 TestFacade1 = Container.ResolveInstance<ITestFacade1>();
                ITestFacade2 TestFacade2 = Container.ResolveInstance<ITestFacade2>();

                Assert.AreEqual(TestFacade1, ((Test2Lazy) TestFacade2).TestFacade1);
            }
        }

        [Test]
        public void component_with_private_repository_shall_only_be_able_to_resolve_components_within_private_repository_when_given_as_parameter()
        {
            ComponentRepository Repository = new ComponentRepository();
            ComponentRepository PrivateRepository = new ComponentRepository();
            Repository.AddComponent<RepositoryParameterTest1>(PrivateRepository, ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {

                ITestFacade1 Test = Container.ResolveInstance<ITestFacade1>();
                Assert.AreEqual(PrivateRepository, ((RepositoryParameterTest1) Test).Repository);
            }
        }

        [Test]
        public void SharedInstance_shall_be_disposed_if_last_container_is_disposed()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<DisposeTest>(ComponentInstanceScope.Repository);

            ComponentContainer Container1 = new ComponentContainer(Repository);
            ComponentContainer Container2 = new ComponentContainer(Repository);


            DisposeTest TestFacade = (DisposeTest) Container1.ResolveInstance<ITestFacade1>();
            Container2.ResolveInstance<ITestFacade1>();

            Container2.Dispose();
            Assert.IsFalse( TestFacade.Disposed );

            Container1.Dispose();
            Assert.IsTrue(TestFacade.Disposed);
        }


        [Test]
        public void SharedInstance_shall_be_disposed_if_last_container_is_disposed_also_with_implicit_dependency()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<DisposeWithDependencyTest>(ComponentInstanceScope.Repository);
            Repository.AddComponent<DisposeTest>(ComponentInstanceScope.Repository);

            ComponentContainer Container1 = new ComponentContainer(Repository);
            ComponentContainer Container2 = new ComponentContainer(Repository);


            DisposeWithDependencyTest TestFacade = (DisposeWithDependencyTest)Container1.ResolveInstance<ITestFacade2>();
            Container2.ResolveInstance<ITestFacade2>();

            Container2.Dispose();
            Assert.IsFalse(TestFacade.Disposed);

            Container1.Dispose();
            Assert.IsTrue(TestFacade.Disposed);
        }

        [Test]
        public void SharedInstance_shall_be_disposed_if_last_container_is_disposed_regardless_in_which_it_was_originally_created()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<DisposeWithDependencyTest>(ComponentInstanceScope.Repository);
            Repository.AddComponent<DisposeTest>(ComponentInstanceScope.Repository);

            ComponentContainer Container1 = new ComponentContainer(Repository);
            ComponentContainer Container2 = new ComponentContainer(Repository);


            DisposeWithDependencyTest TestFacade = (DisposeWithDependencyTest)Container1.ResolveInstance<ITestFacade2>();
            DisposeTest DependentTestFacade = (DisposeTest)Container1.ResolveInstance<ITestFacade1>();
            Container2.ResolveInstance<ITestFacade2>();

            Container1.Dispose();
            Assert.IsFalse(TestFacade.Disposed);
            Assert.Inconclusive("Conceptual issue");
            Assert.IsFalse(DependentTestFacade.Disposed); //Issue: dependent facade dependency is not known when tree is already created. Solution could be to do a dependency walk, but this would fail in case non-shared instances would be involved.

            Container2.Dispose();
            Assert.IsTrue(TestFacade.Disposed);
            Assert.IsTrue(DependentTestFacade.Disposed);
        }

        [Test]
        public void SingletonInstance_shall_be_disposed_when_last_repository_is_disposed()
        {
            ComponentRepository Repository1 = new ComponentRepository();
            Repository1.AddComponent<DisposeTest>(ComponentInstanceScope.Global);

            ComponentRepository Repository2 = new ComponentRepository();
            Repository2.AddComponent<DisposeTest>(ComponentInstanceScope.Global);

            ComponentContainer Container1 = new ComponentContainer(Repository1);
            ComponentContainer Container2 = new ComponentContainer(Repository2);


            DisposeTest TestFacade = (DisposeTest)Container1.ResolveInstance<ITestFacade1>();
            Container2.ResolveInstance<ITestFacade1>();

            Container2.Dispose();
            Assert.IsFalse(TestFacade.Disposed);

            Container1.Dispose();
            Assert.IsTrue(TestFacade.Disposed);
        }

        [Test]
        public void Resolving_of_components_shall_be_possible_from_parent_repository()
        {
            ComponentRepository ParentRepository = new ComponentRepository();
            ParentRepository.AddComponent<Test1>();

            ComponentRepository Repository = new ComponentRepository(ParentRepository);
            Repository.AddComponent<Test2>();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Test2 Instance = (Test2) Container.ResolveInstance<ITestFacade2>();
                Assert.IsInstanceOf<Test1>(Instance.TestFacade1);
            }
        }

        [Test]
        public void If_component_interface_is_defined_in_current_and_parent_repository_current_shall_take_preceedence()
        {
            ComponentRepository ParentRepository = new ComponentRepository();
            ParentRepository.AddComponent<Test1>();

            ComponentRepository Repository = new ComponentRepository(ParentRepository);
            Repository.AddComponent<Test2>();
            Repository.AddComponent<ConfigTest1>(new Config(),Repository); //just to use this 'adder' method also

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Test2 Instance = (Test2)Container.ResolveInstance<ITestFacade2>();
                Assert.IsInstanceOf<ConfigTest1>(Instance.TestFacade1);
            }
        }

        [Test]
        public void Array_dependency_shall_work()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test2B>();
            Repository.AddComponent<Test1>();
            Repository.AddComponent<ConfigTest1>(new Config());

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Test2B Instance = (Test2B)Container.ResolveInstance<ITestFacade2>();

                Assert.AreEqual(2, Instance.Test1.Length);
                Assert.IsInstanceOf<Test1>(Instance.Test1[0]);
                Assert.IsInstanceOf<ConfigTest1>(Instance.Test1[1]);
            }
        }
        [Test]
        public void Progress_shall_be_reported_for_array_dependencies()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test2B>();
            Repository.AddComponent<Test1>();
            Repository.AddComponent<ConfigTest1>(new Config());



            List<Tuple<Tuple<int, int>, string>> Progress = new List<Tuple<Tuple<int, int>, string>>();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Container.ResolveInstance<ITestFacade2>((max, cur, name) => Progress.Add(new Tuple<Tuple<int, int>, string>(new Tuple<int, int>(max, cur), name)));
            }

            Assert.That(Progress.Count, Is.EqualTo(3));
            Assert.That(Progress[0].Item1.Item1, Is.EqualTo(3));
            Assert.That(Progress[0].Item1.Item2, Is.EqualTo(1));
            Assert.That(Progress[0].Item2, Is.EqualTo("Test2B"));
            Assert.That(Progress[1].Item1.Item1, Is.EqualTo(3));
            Assert.That(Progress[1].Item1.Item2, Is.EqualTo(2));
            Assert.That(Progress[1].Item2, Is.EqualTo("Test1"));  
            Assert.That(Progress[2].Item1.Item1, Is.EqualTo(3));
            Assert.That(Progress[2].Item1.Item2, Is.EqualTo(3));
            Assert.That(Progress[2].Item2, Is.EqualTo("ConfigTest1"));
        }

        [Test]
        public void Array_dependency_shall_also_work_when_no_component_is_providing_the_interface()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test2B>();

            using(ComponentContainer Container = new ComponentContainer(Repository))
            {
                Test2B Instance = (Test2B) Container.ResolveInstance<ITestFacade2>();

                Assert.AreEqual(0, Instance.Test1.Length);
            }
        }

        [Test]
        public void Exception_during_costruction_shall_not_do_collateral_damage_sharedcomponent()
        {
            // in another test case, an exception was thrown in dispose because the instance did not construct properly.
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RecursionTest1>();
            Repository.AddComponent<RecursionTest2>();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Assert.Throws<InvalidOperationException>(() => Container.ResolveInstance<ITestFacade1>());
            }
        }
        
        [Test]
        public void Exception_during_costruction_shall_not_do_collateral_damage_simplecomponent()
        {
            // in another test case, an exception was thrown in dispose because the instance did not construct properly.
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RecursionTest1>(ComponentInstanceScope.Container);
            Repository.AddComponent<RecursionTest2>(ComponentInstanceScope.Container);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Assert.Throws<InvalidOperationException>(() => Container.ResolveInstance<ITestFacade1>());
            }
        }

        [Test]
        public void Exception_during_costruction_shall_not_do_collateral_damage_singletoncomponent()
        {
            // in another test case, an exception was thrown in dispose because the instance did not construct properly.
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RecursionTest1>(ComponentInstanceScope.Global);
            Repository.AddComponent<RecursionTest2>(ComponentInstanceScope.Global);

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Assert.Throws<InvalidOperationException>(() => Container.ResolveInstance<ITestFacade1>());
            }
        }

        [Test]
        public void Exception_shall_be_thrown_if_no_suitable_constructor_is_found()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<NoSuitableConstructor>();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Assert.Throws<ResolveComponentException>(() => Container.ResolveInstance<ITestFacade1>());
            }
        }

        [Test]
        public void Recursion_shall_be_detected()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<RecursionTest1>();
            Repository.AddComponent<RecursionTest2>();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Assert.Throws<InvalidOperationException>(() => Container.ResolveInstance<ITestFacade1>());
            }
        }

        [Test]
        public void Resolve_shall_include_external_instances()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test2>();
            Test1 Test1 = new Test1();

            using (ComponentContainer Container = new ComponentContainer(Repository, Test1))
            {
                Test2 Test2 = (Test2) Container.ResolveInstance<ITestFacade2>();

                Assert.AreEqual(Test1, Test2.TestFacade1);
            }
        }

        [Test]
        public void Disposable_container_shall_not_be_useable_anymore()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<Test1>();

            ComponentContainer Container = new ComponentContainer(Repository);
            Container.Dispose();

            Assert.Throws<ObjectDisposedException>(() => Container.ResolveInstance<ITestFacade1>());
        }

        [Test]
        public void Component_resolve_shall_only_work_with_componentinterface()
        {
            ComponentRepository Repository = new ComponentRepository();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Assert.Throws<ArgumentException>(() => Container.ResolveInstance<INoComponentInterfaceFacade>());
            }
        }

        [Test]
        public void Component_try_resolve_shall_only_work_with_componentinterface()
        {
            ComponentRepository Repository = new ComponentRepository();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Assert.Throws<ArgumentException>(() => Container.TryResolveInstance<INoComponentInterfaceFacade>());
            }
        }

        [Test]
        public void Component_try_resolve_shall_return_null_if_no_component_is_found()
        {
            ComponentRepository Repository = new ComponentRepository();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                ITestFacade1 Instance = Container.TryResolveInstance<ITestFacade1>();
                Assert.That(Instance, Is.Null);
            }
        }

        [Test]
        public void ComponentArray_resolve_shall_only_work_with_componentinterface()
        {
            ComponentRepository Repository = new ComponentRepository();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Assert.Throws<ArgumentException>(() => Container.ResolveInstances<INoComponentInterfaceFacade>());
            }
        }
    
        [Test]
        public void Component_shall_only_be_accepted_with_proper_attribute()
        {
            ComponentRepository Repository = new ComponentRepository();
            Assert.Throws<ArgumentException>(Repository.AddComponent<NoComponent>);
        }

        [Test]
        public void Component_resolve_shall_be_possible_with_delegated_property()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<DelegatedTest>();

            using (ComponentContainer Container = new ComponentContainer(Repository))
            {
                Test1 Test1 = (Test1)Container.ResolveInstance<ITestFacade1>();

                Assert.IsNotNull(Test1);
            }
        }
    }
}