object = {}
function object:new()
-- object is parameter
-- return a table

	local obj = {}
	-- metat table __index
	self.__index = self
	setmetatable(obj,self)
	return obj
end
function object:subClass( className )
-- _G知识点 总表 所有声明的键值对都在这里面存着
	_G[className] = {}
	--继承的规则
	--用元表的只是
	local obj = _G[className]
	self.__index = self
	-- 定义一个base属性，base属性代表父类
	obj.base = self
	setmetatable(obj,self)
end
require("BuffManager")