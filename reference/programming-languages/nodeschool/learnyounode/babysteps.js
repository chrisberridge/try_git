var nums = process.argv;
var sum = 0;
for (var index = 2; index < nums.length; index++) {
	var element = nums[index];
	sum += Number(element);
}
console.log(sum);