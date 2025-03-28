using Sandbox;
using Sandbox.Diagnostics;

namespace Spring.Utils
{
    public static class GameObjectExtensions
    {
		public static T MustGetComponent<T>(this Component pComponent) where T : Component
		{
			T component = pComponent.GetComponent<T>();
			Assert.NotNull(component, $"Cannot get component '{typeof(T).Name}' of '{pComponent.GameObject.Name}'");
			return component;
		}

        public static T MustGetComponentInChildren<T>(this Component pComponent) where T : Component
        {
            T component = pComponent.GetComponentInChildren<T>();
            Assert.NotNull(component, $"Cannot get component '{typeof(T).Name}' of '{pComponent.GameObject.Name}' children");
            return component;
        }

        public static T MustGetComponentInParent<T>(this Component pComponent) where T : Component
        {
            T component = pComponent.GetComponentInParent<T>();
            Assert.NotNull(component, $"Cannot get component '{typeof(T).Name}' of '{pComponent.GameObject.Name}' parent");
            return component;
        }

        public static T MustGetComponent<T>(this GameObject pGameObject) where T : Component
        {
            T component = pGameObject.GetComponent<T>();
            Assert.NotNull(component, $"Cannot get component '{typeof(T).Name}' of '{pGameObject.Name}'");
            return component;
        }
    }
}
