Smooth Operator allows the safe use of the null contional (?.) and null coalescing (??) operators on UnityObjects. 

Once package is added to the project, no further configuration is required. 

Usage: 

Replace code like this:

if(transform != null) {
{
    transform.Translate(Vector3.right);
}

with this:

transform?.Translate(vector3.right);