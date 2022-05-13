﻿using System;
using Monai.Deploy.WorkflowManager.ConditionsResolver.Resolver;
using Xunit;

namespace Monai.Deploy.WorkflowManager.ConditionsResolver.Tests.Resolver
{
    public class ConditionalTests
    {
        [Theory]
        [InlineData("{{context.dicom.tags[('0010','0040')]}}", "F", "{{context.dicom.tags[('0010','0040')]}} == 'F'")]
        [InlineData("{{context.executions.body_part_identifier.result.body_part}}", "leg", "{{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData("F", "F", "'F' == 'F'")]
        [InlineData("F", "{{context.dicom.tags[('0010','0040')]}}", "'F' == {{context.dicom.tags[('0010','0040')]}}")]
        public void Conditional_CreatesAndEvaluates(string expectedLeftParam, string expectedRightParam, string input)
        {
            var conditional = Conditional.Create(input);
            var leftParameter = conditional.LeftParameter;
            var rightParameter = conditional.RightParameter;

            Assert.Equal(expectedLeftParam, leftParameter);
            Assert.Equal(expectedRightParam, rightParameter);
        }

        [Theory]
        [InlineData("AND {{context.dicom.tags[('0010','0040')]}} == 'F'", "No left hand parameter at index: 0")]
        [InlineData(" AND {{context.dicom.tags[('0010','0040')]}} == 'F'", "No left hand parameter at index: 0")]
        [InlineData("OR {{context.dicom.tags[('0010','0040')]}} == 'F'", "No left hand parameter at index: 0")]
        [InlineData(" OR {{context.dicom.tags[('0010','0040')]}} == 'F'", "No left hand parameter at index: 0")]
        public void Conditional_WhenGivenInvalidInput_ShouldThrowErrors(string input, string expectedErrorMessage)
        {
            var exception = Assert.Throws<ArgumentException>(() => Conditional.Create(input));

            Assert.Equal(expectedErrorMessage, exception.Message);
        }

    }
}
