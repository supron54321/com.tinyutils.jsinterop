mergeInto(LibraryManager.library, {
    _InitializeInterop: function(mallocPtr, freePtr){
        console.log("JsBridge initialized");
        window.Interop = {
            functionArray: [],
            
            HeapAlloc: function(size){
                return dynCall_ii(mallocPtr, size);
            },
            HeapFree: function(address){
                dynCall_vi(freePtr, address);
            },
            
            DeserializeFromHeap: function(argPtr, argSize){
                var array = HEAP8.subarray(argPtr, argPtr+argSize);
                var arguments = window.tinyMsgPack.deserialize(array);
                return arguments;
            },
            SerializeAndStoreOnHeap: function(returnObject){
                var serialized = window.tinyMsgPack.serialize(returnObject);
                var bufferPtr = this.HeapAlloc(serialized.length+4);
                HEAP32[bufferPtr>>2] = serialized.length;
                HEAP8.set(serialized, (bufferPtr+4));
                
                return bufferPtr;
            },
            
            ReturnThrow: function(error){
                return this.SerializeAndStoreOnHeap([
                    [error.name, error.message, error.stack],
                    null
                ]);
            },
            
            ReturnSuccess: function(result){
                return this.SerializeAndStoreOnHeap([
                    null,
                    result
                ]);
            }
        }
    },
    _RegisterJsFunction: function(pathPtr){
        var pathString = UTF8ToString(pathPtr);
        
        var funcPtr = window.Interop.functionArray.length;
        window.Interop.functionArray.push(Function('args', 'return '+pathString+'(...args);'));
        return funcPtr;
    },
    _CallJsFunction: function(functionPtr, argsPtr, argsLength) {
        var fcn = window.Interop.functionArray[functionPtr];
        if(!fcn)
            return window.Interop.ReturnThrow(new Error("Invalid function pointer "+functionPtr));
        try{
            var ret = fcn(window.Interop.DeserializeFromHeap(argsPtr, argsLength));
            return window.Interop.ReturnSuccess(ret);
        }
        catch(e) {
            return window.Interop.ReturnThrow(e);
        }
        
    },
    _RegisterJsCallback: function(pathPtr, functionPtr){
        var path = UTF8ToString(pathPtr);
        var splitPath = path.split('.');
        
        if(splitPath[0] == "window")
            splitPath.splice(0, 1);
        
        var objRef = window;
        for(var i = 0; i < splitPath.length-1; i++)
        {
            var thisField = objRef[splitPath[i]];
          if(thisField == undefined)
          {
            objRef[splitPath[i]] = [];
          }
          objRef = objRef[splitPath[i]];
        }
        
        objRef[splitPath[splitPath.length-1]] = function(){
            var args = [].slice.call(arguments);
            var argsPtr = window.Interop.SerializeAndStoreOnHeap(args);
            var retPtr = dynCall_ii(functionPtr, argsPtr);
            if(retPtr != 0)
            {
                var retArray = window.Interop.DeserializeFromHeap(retPtr+4, HEAP32[retPtr>>2]);
                if(retArray[0] != null)
                {
                    var error = new Error(retArray[0][1]);
                    error.name = retArray[0][0];
                    error.stack = retArray[0][2];
                    throw error;
                }
                return retArray[1];
            }
            else{
                throw new Error("JsInterop - callback returned null pointer");
            }
        };
    }
});
