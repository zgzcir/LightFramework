# LightFramework
# Unity的资源加载框架

## 使用方式：

### 1，导入UnityPackage，进行配置

使用已有的FrameProfile配置文件或右键Create FrameProfile进行创建

![avatar](https://i.loli.ma/pic/5d618444587da4baa6736df8a1374a46.png)

### 2，ab包配置方式：(主要分为两种配置方式)
  使用已有的AssetBundleBuildProfile配置文件或右键Create AssetBundleBuildProfile进行创建

![Snipaste_2020-01-10_14-35-18.png](https://i.loli.ma/pic/b545dd59ab0b74938beceb63aeee2188.png)
  
  

   Asset Directories为资源文件夹设置，设置的时候需要设置ab包名与ab包对应文件夹路径

   AssetBundle Load Profile Directory为打包后生成的配置表，供加载时读取，放在默认文件夹下，无需修改。

  
   
### 3，资源加载代码使用：

  #### 1）同步资源加载：
  ```
ResourceManager.Instance.LoadResource<T>(path)
  ```
  泛型方法，path为资源的Unity工程相对路径，如： Assets/Data/image.png;。此方法加载不需要实例化的资源，如图片，asset,音频等资源文件。
  ```
  示例：
  TextAsset text = ResourceManager.Instance.LoadResource<TextAsset>(MAINLUAPATH);
  ```
	
  #### 2）异步资源加载：
  ```
  ResourceManager.Instance.AsyncLoadResource(string path, OnAsyncFinish cb, LoadResPriority priority, bool isSprite = false, uint crc = 0,params object[] paramList) 
  ```
  异步资源加载函数，此方法加载不需要实例化的资源，如图片，音频等资源文件。 path为资源的Unity工程相对路径，如： Assets/Data/image.png； cb 为加载回调；priority为加载优先级；paramList 为参数，可以向回调传递参数。 crc为资源路径计算出来的crc，如果计算了crc那么就会强制根据crc查找，不然会根据路径进行查找资源。isSprite为是否是Sprite，因为异步资源加载图片有特殊转换。
   ```
  回调委托定义：
delegate void OnAsyncFinish(string path, Object obj, params object[] paramList);
  ```
  
  	```
  	示例：
	   ResourceManager.Instance.AsyncLoadResource(path, (p, obj, paramList) =>
            {
                Debug.Log(obj.name);
            },LoadResPriority.RES_MIDDLE);
  	```
  #### 3）释放资源：
  ```
  ResourceManager.Instance.ReleaseResource(Object obj, bool isDestroyCache = false)
 
   ResourceManager.Instance.ReleaseResource(string path, bool isDestroyCache = false)
  ```
  同步资源加载的释放方法，isDestroyCache为是否完全释放改资源，false则不从内存中卸载，仅减少资源引用计数。
  ```
  示例：
  ResourceManager.Instance.ReleaseResource(BgmSource.clip, true);
  ```
  
  #### 4）预加载资源：
  ```
  ResourceManager.Instance.PreLoadRes(string path)
  ```
  预加载资源，传入路径即可进行预加载。
  ```
  示例：
  ResourceManager.Instance.PreloadResource(MAINLUAPATH);
  ```
  
  #### 5）同步实例化gameobject
  ```
  ObjectManager.Instance. InstantiateObject(string path, bool isShowDefaultTrans = false, bool isClear = true)
  ```
  同步gameobject加载，path为prefab路径；isShowDefaultTrans为是否放在默认节点下面；isClear跳转场景时是否清除。
  ```
  示例：
  GameObject obj =  ObjectManager.Instance.InstantiateObject(Constans.HPItem);
  ```
  
  #### 6）异步实例化gameobject
  ```
  ObjectManager.Instance.(string path, OnAsyncFinish callBack, LoadResPriority priority,
            bool isSetSceneTrans = false, bool isClear = true, params object[] paramList)
  ```
  异步gameobject加载path为prefab路径；callBack为加载回调；priority为加载优先级;isSetSceneTrans为是否放到Scene节点下面；paramList 为参数，可以向回调传递参数。
  ```
回调委托定义：
delegate void OnAsyncFinish(string path, Object obj, params object[] paramList);
 
  示例：
       ObjectManager.Instance.AsyncInstantiateObject(path, (_path,obj,paramList) =>
            {
                Debug.Log(obj.name);
            }, LoadResPriority.RES_HIGH);
  ```
  
  #### 7）卸载gameobject
  ```
  ObjectManager.Instance.ReleaseObject(GameObject obj, int maxCacheCount = -1, bool isDestroyPrimitiveCache = false,
            bool isRecyleParent = true)
  ```
  参数obj为实例化的gameobject引用；maxCacheCount为缓存最大个数，-1代表无限；isDestroyPrimitiveCache为是否清除缓存；isRecyleParent为是否放回回收节点，如果不放回自动Setactive为false，一般用于UI的prefab释放，减少setparent操作。
  ```
  示例：
  ObjectManager.Instance.ReleaseObject(gameObject);
  ```
  
  #### 7）预加载gameobject
  ```
  ObjectManager.Instance. PreLoadObject(string path, int count = 1, bool isClear = false)
  ```
  预加载gamobject,path为Prefab路径；count为预加载个数；isClear为跳场景是否清除。
  ```
  示例：
  ObjectManager.Instance.PreloadGameObject(path, 40);
  ```
  
  #### 8）跳场景时对缓存的清除
  ```
  ObjectManager.Instance.ClearCache();
  ResourceManager.Instance.ClearCache();
  ```
  
  调用这两个函数，在资源加载的时候有参数来确定某些资源或者Prefab跳场景是否清除，如果不清除，将常驻内存，方便快速加载。
  

  
  #### 9）数据配置
  此框架包含了数据配置功能，实现了类与xml与二进制与Excel表之间的互转，xml与Excel表之间的转换由一个如下的reg表定义：
    ![reg.png](https://i.loli.net/2020/01/10/PAmrIXqQB37FLHY.png)
      可通过实现IRegTypeParser接口并在parsers.json中进行配置来实现自定义的类型解析。

  
  
