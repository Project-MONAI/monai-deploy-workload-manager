using Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver;
using Xunit;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Tests.Resolver
{
    public class ConditionalGroupTests
    {
        //TODO: Multiple AND/OR Keywords - DONE
        //TODO: Multiple lowercase AND/OR Keywords
        //TODO: Error Conditions AND/OR Keywords
        //TODO: Test Operators ('==') AND/OR Keywords
        //TODO: Parse Parameters
        //TODO: Test reversed parameters 'F' on left and {{}} on right

        [Theory]
        [InlineData("{{context.dicom.tags[('0010','0040')]}} == 'F' AND {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData("{{context.dicom.tags[('0010','0040')]}} == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData("'F' == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData("'F' == 'F' OR 'F' == 'leg'")]
        [InlineData("'AND' == 'F' OR 'F' == 'leg'")]
        [InlineData("'OR' == 'F' OR 'F' == 'leg'")]
        [InlineData("'F' == 'F' or 'F' == 'leg'")] // Lowercase OR
        [InlineData("'F' == 'F' AND 'F' == 'leg'")] // Lowercase AND
        [InlineData("'F' == 'F' AND 'F' == 'leg'")]
        [InlineData("'LEG' == 'F' OR 'F' == 'leg'")]
        [InlineData("'F' == 'F' OR 'F' == 'leg' AND 'F' == 'F'")]
        [InlineData("'F' == 'F' AND 'F' == 'leg' AND 'F' == 'F'")]
        [InlineData("'F' == 'F' AND 'F' == 'leg' OR 'F' == 'F'")]
        [InlineData("'F' == 'F' OR 'F' == 'leg' OR 'F' == 'F'")]
        [InlineData("'F' == 'F' OR 'F' == 'leg' OR 'F' == 'F' AND 'F' == 'F'")]
        [InlineData("'F' == 'F' OR 'F' == 'leg' OR 'F' == 'F' AND 'F' == 'F' AND 'F' == 'F'")]
        [InlineData("'F' == 'F' AND 'F' == 'leg' OR 'F' == 'F' AND 'F' == 'F' OR 'F' == 'F'")]
        [InlineData("'F' == 'F' AND 'F' == 'leg' OR 'F' == 'F' OR 'F' == 'F' AND 'F' == 'F'")]
        [InlineData("'AND' == 'OR' AND 'F' == 'leg' OR 'F' == 'F' OR 'F' == 'F' AND 'F' == 'F'")]
        public void ConditionalGroup_WhenProvidedCorrectInput_ShouldCreateAndHaveLeftAndRightGroups(string input)
        {
            var conditionalGroup = ConditionalGroup.Create(input);
            Assert.True(conditionalGroup.LeftIsSet);
            Assert.True(conditionalGroup.RightIsSet);
        }

        [Theory]
        //[InlineData(false, "{{context.dicom.tags[('0010','0040')]}} == 'F' AND {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        //[InlineData(false, "{{context.dicom.tags[('0010','0040')]}} == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData(true, "'F' == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")] // this is great doesn't even evaluate right hand param because left hand side passes!
        [InlineData(true, "'F' == 'F' OR 'F' == 'leg'")]
        [InlineData(true, "'LEG' == 'F' OR 'leg' == 'leg'")]
        [InlineData(true, "'1' == '1' OR 'donkey' == 'leg'")]
        [InlineData(true, "'5' > '1' AND 'donkey' == 'donkey'")]
        [InlineData(false, "'5' < '1' AND 'donkey' == 'donkey'")] // 5 less than 1
        [InlineData(false, "'5' > '1' AND 'Donkey' == 'donkey'")] // capital D in donkey
        [InlineData(true, "'5' > '1' AND 'Donkey' != 'donkey'")]
        [InlineData(true, "'5' => '5' AND 'Donkey' != 'donkey'")]
        [InlineData(false, "'5' >= '5' AND 'Donkey' != 'donkey'")]
        public void ConditionalGroup_WhenProvidedCorrectInput_ShouldCreateAndEvaluate(bool expectedResult, string input)
        {
            var conditionalGroup = ConditionalGroup.Create(input);
            Assert.NotNull(conditionalGroup.LeftConditional);
            Assert.NotNull(conditionalGroup.RightConditional);
            var result = conditionalGroup.Evaluate();
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(
            "{{context.dicom.tags[('0010','0040')]}}",
            "F",
            "{{context.executions.body_part_identifier.result.body_part}}",
            "leg",
            "{{context.dicom.tags[('0010','0040')]}} == 'F' AND {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData(
            "{{context.dicom.tags[('0010','0040')]}}",
            "F",
            "{{context.executions.body_part_identifier.result.body_part}}",
            "leg",
            "{{context.dicom.tags[('0010','0040')]}} == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        public void ConditionalGroup_Creates_HasLeftAndRightGroupsWithValues(string leftGroupLeftParam,
                                                                             string leftGroupRightParam,
                                                                             string rightGroupLeftParam,
                                                                             string rightGroupRightParam,
                                                                             string input)
        {
            var conditionalGroup = ConditionalGroup.Create(input);
            Assert.NotNull(conditionalGroup.LeftConditional);
            Assert.NotNull(conditionalGroup.RightConditional);

            Assert.Equal(leftGroupLeftParam, conditionalGroup?.LeftConditional?.LeftParameter);
            Assert.Equal(leftGroupRightParam, conditionalGroup?.LeftConditional?.RightParameter);
            Assert.Equal(rightGroupLeftParam, conditionalGroup?.RightConditional?.LeftParameter);
            Assert.Equal(rightGroupRightParam, conditionalGroup?.RightConditional?.RightParameter);
        }
    }
}
